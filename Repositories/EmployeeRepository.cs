using DACN.Data;
using DACN.DTOs.Request;
using DACN.DTOs.Respone;
using DACN.Enums;
using DACN.Models;
using Microsoft.EntityFrameworkCore;
using static DACN.Enums.StatusEnums;

namespace DACN.Repositories
{
    public class EmployeeRepository
    {
        private readonly ApplicationDbContext db;

        public EmployeeRepository(ApplicationDbContext db)
        {
            this.db = db;
        }
        public async Task<EmployeeModel?> GetByIdAsync(int id)
        {
            return await db.Employees
              
                .Include(e => e.Account)
                    .ThenInclude(a => a.JobApplications)
                        .ThenInclude(j => j.JobPosting)
                            .ThenInclude(p => p.CreatedBy) 
                                .ThenInclude(cb => cb.Account) 
                .Include(e => e.Account)
                    .ThenInclude(a => a.EducationExperiences)
                .Include(e => e.Department)
                .Include(e => e.Position)
                .Include(e => e.Contracts)

                .FirstOrDefaultAsync(e => e.EmployeeId == id);
        }

        public async Task<EmployeeModel?> GetByEmailAsync(string email)
        {
            return await db.Employees
                           .Include(e => e.Account) // chắc chắn load Account
                           .FirstOrDefaultAsync(e => e.Account.Email == email);
        }
        public async Task<string?> GetNameByIdAsync(int id)
        {
            return await db.Employees
                           .Where(e => e.EmployeeId == id)
                           // Chỉ trỏ đến đúng cột cần lấy (SQL sẽ chỉ Select column này)
                           .Select(e => e.Account.FullName)
                           .FirstOrDefaultAsync();
        }
        public async Task<EmployeeModel?> GetByUserAccount(int accountId)
        {
            return await db.Employees
                           .Include(e => e.Account) // chắc chắn load Account
                           .FirstOrDefaultAsync(e => e.Account.UserAccountId == accountId);
        }
        public async Task<List<EmployeeModel>> GetAllEmployee()
        {
            return await db.Employees
                           .Include(e => e.Account)
                           .Include(e => e.Department)
                           .Include(e => e.Position)
                           .Where(e => !e.IsDeleted)
                           .ToListAsync();
        }

        public async Task Update(EmployeeModel employee)
        {
            var existingEmployee = await db.Employees.FindAsync(employee.EmployeeId);
            if (existingEmployee == null)
            {
                throw new Exception("Employee not found");
            }
            existingEmployee.Account.FullName = employee.Account.FullName;
            existingEmployee.Account.Email = employee.Account.Email;
            existingEmployee.Phone = employee.Phone;
            existingEmployee.Address = employee.Address;
            existingEmployee.DateOfBirth = employee.DateOfBirth;
            existingEmployee.CCCD = employee.CCCD;
            existingEmployee.Gender = employee.Gender;
            existingEmployee.UpdatedAt = DateTime.Now;
            db.Employees.Update(employee);
            await db.SaveChangesAsync();
        }

        public async Task<EmployeeModel> CreateEmployeeAsync(EmployeeRequest dto)
        {
            var emp = new EmployeeModel
            {
                UserAccountId = dto.UserAccountId,
                IsActive = true,
                CreatedAt = DateTime.Now,
                Status = EmployeeStatus.ChuaNhanViec, // trạng thái mới
                PositionId = dto.PositionId,
                DepartmentId = dto.DepartmentId,
                DateOfBirth = dto.DateOfBirth,
                Address = dto.Address,
                Phone = dto.Phone,
                CCCD = dto.CCCD,
                AvatarUrl = dto.avatarUrl, // chú ý chữ A hoa
                StartDate = dto.StartDate
            };

            db.Employees.Add(emp);
            await db.SaveChangesAsync();

            return emp; // trả về EmployeeModel vừa tạo
        }
        public async Task<(List<EmployeeRespone> Data, int Total)> GetPagedAsync(
        int page, int pageSize, string keySearch, int IsActive, int departmentId, int positionId,
         string? cccd, string? email, DateTime? fromDate, DateTime? DateTo)
        {
            var query = db.Employees
             .Include(j => j.Account)
             .Include(j => j.Position)
              .Include(j => j.Department)
             .Where(j => !j.IsDeleted && j.Account.Role == UserRole.Employee)
             .AsQueryable();

            if (!string.IsNullOrEmpty(keySearch))
                query = query.Where(j => j.Account.FullName.Contains(keySearch));
            if (!string.IsNullOrEmpty(cccd))
                query = query.Where(j => j.CCCD != null && j.CCCD.Contains(cccd));
            if (!string.IsNullOrEmpty(email))
                query = query.Where(j => j.Account.Email.Contains(email));
            if (departmentId != 0)
                query = query.Where(j => j.DepartmentId == departmentId);
            if (positionId != 0)
                query = query.Where(j => j.PositionId == positionId);
            if (fromDate.HasValue)
                query = query.Where(j => j.CreatedAt >= fromDate.Value);
            if (DateTo.HasValue)
                query = query.Where(j => j.CreatedAt <= DateTo.Value);
            if (IsActive != -1)
                query = query.Where(j => j.IsActive == (IsActive == 1));
            var total = await query.CountAsync();
            var data = await query
                .OrderByDescending(j => j.CreatedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                 .Select(j => new EmployeeRespone
                 {
                     EmployeeId = j.EmployeeId,
                     FullName = j.Account.FullName,
                     Email = j.Account.Email,
                     Phone = j.Phone,
                     DepartmentName = j.Department != null ? j.Department.DepartmentName : string.Empty,
                     PositionName = j.Position != null ? j.Position.PositionName : string.Empty,
                     DateOfBirth = j.DateOfBirth,
                     CCCD = j.CCCD,
                     Address = j.Address,
                     StartDate = j.StartDate,
                     CreatedAt = j.CreatedAt,
                     IsActive = j.IsActive,
                     AvatarUrl = j.AvatarUrl,
                 })
                .ToListAsync();
            return (data, total);
        }
        public async Task AddWithEducationAsync(JobApplicationModel jobApp, EducationExperienceModel? edu = null)
        {

            using var transaction = await db.Database.BeginTransactionAsync();
            try
            {
                if (edu != null)
                {
                    db.educationExperiences.Add(edu);
                }

                db.JobApplications.Add(jobApp);

                // Lưu cả 2 thay đổi vào DB
                await db.SaveChangesAsync();
                // Commit transaction
                await transaction.CommitAsync();
            }
            catch
            {
                // Rollback nếu lỗi
                await transaction.RollbackAsync();
                throw;
            }
        }

        public async Task UpdateEmployeeAndExperienceAsync(EmployeeRequest dto)
        {
            // Bắt đầu Transaction để cập nhật 2 bảng
            using var transaction = await db.Database.BeginTransactionAsync();
            try
            {
                // === 1. CẬP NHẬT EMPLOYEE ===
                // (Dùng dto.EmployeeId để tìm)
                var emp = await db.Employees.FirstOrDefaultAsync(e => e.EmployeeId == dto.EmployeeId);

                if (emp == null)
                {
                    throw new Exception("Không tìm thấy nhân viên để cập nhật.");
                }

                // Cập nhật các trường
                emp.Phone = dto.Phone;
                emp.DepartmentId = dto.DepartmentId;
                emp.PositionId = dto.PositionId;
                emp.Status = dto.Status;
                emp.Gender = dto.Gender;
                emp.CCCD = dto.CCCD;
                emp.Address = dto.Address;
                emp.DateOfBirth = dto.DateOfBirth;
                emp.StartDate = dto.StartDate;

                // Không cần 'db.Entry(emp).State = EntityState.Modified;'
                // EF Core sẽ tự động theo dõi thay đổi khi bạn lấy nó ra (FirstOrDefaultAsync)


                // === 2. CẬP NHẬT EDUCATIONEXPERIENCE ===
                // (Dùng dto.EducationExperienceId để tìm)

                if (dto.EducationExperienceId.HasValue && dto.EducationExperienceId > 0)
                {
                    // Tìm bản ghi học vấn bằng ID
                    var eduExp = await db.educationExperiences // (Tên bảng Education của bạn)
                        .FirstOrDefaultAsync(edu => edu.Id == dto.EducationExperienceId);

                    if (eduExp != null)
                    {
                        // Cập nhật các trường từ DTO
                        eduExp.EducationLevel = dto.EducationLevel.Value; // Cần kiểm tra null
                        eduExp.University = dto.University;
                        eduExp.Major = dto.Major;
                        eduExp.GPA = dto.GPA; // Cần kiểm tra null
                        eduExp.GraduationYear = dto.GraduationYear;
                        eduExp.ExperienceDescription = dto.ExperienceDescription;
                    }
                }
                await db.SaveChangesAsync();

                await transaction.CommitAsync();
            }
            catch (Exception)
            {
                // Nếu có bất kỳ lỗi nào, rollback tất cả
                await transaction.RollbackAsync();
                throw; // Ném lỗi ra ngoài để controller xử lý
            }



        }
        // Sửa kiểu trả về từ Task thành Task<UserAccountModel>
        public async Task<UserAccountModel> addEmployee(EmployeeRequest dto)
        {
            using var transaction = await db.Database.BeginTransactionAsync();
            try
            {
                // === BƯỚC 1: TẠO ACCOUNT ===
                var userAccount = new UserAccountModel
                {
                    FullName = dto.FullName,
                    Email = dto.Email,
                    CreatedAt = DateTime.Now,
                    Role = UserRole.Employee,

                    // 1. Sửa thành false (chờ kích hoạt)
                    IsActive = false,

                    PasswordHash = null, // Chưa có mật khẩu

                    ActivationToken = Guid.NewGuid().ToString(),

                    // 2. Thêm hạn sử dụng cho Token (24h)
                    TokenExpiry = DateTime.Now.AddHours(24)
                };

                db.UserAccounts.Add(userAccount);

                // Lưu lần 1 để sinh ID
                await db.SaveChangesAsync();

                int newAccountId = userAccount.UserAccountId;

                // === BƯỚC 2: TẠO EMPLOYEE ===
                var newEmployee = new EmployeeModel
                {
                    UserAccountId = newAccountId, // Gán ID vừa tạo

                    DepartmentId = dto.DepartmentId,
                    PositionId = dto.PositionId,
                    Phone = dto.Phone,
                    CCCD = dto.CCCD,
                    Address = dto.Address,
                    Gender = dto.Gender,
                    DateOfBirth = dto.DateOfBirth,
                    StartDate = dto.StartDate,
                    Status = dto.Status,
                    CreatedAt = DateTime.Now,
                    AvatarUrl = "/images/default-avatar.png", // Ảnh mặc định
                    IsActive = true, // Nhân viên thì active (nhưng tk đăng nhập thì chưa)
                };
                db.Employees.Add(newEmployee);

                // === BƯỚC 3: TẠO EDUCATION (Nếu có) ===
                if (dto.EducationLevel.HasValue || !string.IsNullOrEmpty(dto.University))
                {
                    var newEducation = new EducationExperienceModel
                    {
                        UserAccountId = newAccountId, // Gán ID Account

                        EducationLevel = dto.EducationLevel ?? 0,
                        University = dto.University,
                        Major = dto.Major,
                        GraduationYear = dto.GraduationYear,
                        GPA = dto.GPA,
                        ExperienceDescription = dto.ExperienceDescription
                        // Nếu có Skills thì gán thêm: Skills = dto.Skills
                    };
                    db.educationExperiences.Add(newEducation);
                }

                // === BƯỚC 4: LƯU VÀ COMMIT ===
                await db.SaveChangesAsync();
                await transaction.CommitAsync();

                // 3. Trả về userAccount để Controller lấy Token gửi email
                return userAccount;
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                throw;
            }
        }
        public async Task<UserAccountModel?> GetByActivationTokenAsync(string token)
        {
            return await db.UserAccounts
                .FirstOrDefaultAsync(u => u.ActivationToken == token);
        }

        public async Task UpdateAsync(UserAccountModel user)
        {
            db.UserAccounts.Update(user); // Hoặc _context.Entry(user).State = EntityState.Modified;
            await db.SaveChangesAsync();
        }
        public async Task<List<EmployeeModel>> GetEmployeesByDepartmentAsync()
        {
            return await db.Employees
                           .Include(e => e.Account)
                           .Include(e => e.Department)
                           .Where(e => e.IsActive && !e.IsDeleted && e.Account.Role== UserRole.Employee)
                           .Where(e => !e.Contracts.Any(c => c.Status == ContractStatus.ConHieuLuc))
                           .ToListAsync();
        }
    }
}


