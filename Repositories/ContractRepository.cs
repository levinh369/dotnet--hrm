using DACN.Data;
using DACN.DTOs.Request;
using DACN.DTOs.Respone;
using DACN.Models;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics.Contracts;
using static DACN.Enums.StatusEnums;

namespace DACN.Repositories
{
    public class ContractRepository
    {
        private readonly ApplicationDbContext db;

        public ContractRepository(ApplicationDbContext db)
        {
            this.db = db;
        }
        public async Task<List<ContractModel>> GetAllAsync()
        {
            return await db.Contracts.ToListAsync();
        }
        public async Task<ContractModel?> GetByIdAsync(int id)
        {
            return await db.Contracts
                .Include(c => c.Employee)
                    .ThenInclude(e => e.Account)
                .Include(c => c.Employee.Department)
                 .Include(c => c.Employee.Position)
                .FirstOrDefaultAsync(j => j.ContractId == id);
        }
        public async Task<(List<ContractModel> Data, int Total)> GetPagedAsync(
        int page, int pageSize, string keySearch, DateTime? fromDate, DateTime? toDate, int contractType, int status)
        {
            var query = db.Contracts
             .AsNoTracking()
             .Include(c => c.Employee)
                 .ThenInclude(e => e.Account)
             .Include(c => c.Employee.Department)
             .Include(c => c.Employee.Position)
             .Where(c => !c.IsDeleted );

            if (!string.IsNullOrEmpty(keySearch))
                query = query.Where(c => c.ContractCode.Contains(keySearch) || c.Employee.Account.FullName.Contains(keySearch) || c.Employee.CCCD.Contains(keySearch));

            if (fromDate.HasValue)
                query = query.Where(c => c.SignedDate >= fromDate.Value);

            if (toDate.HasValue)
                query = query.Where(c => c.SignedDate <= toDate.Value);

            if (status != -1)
            {
                if (status == 1)
                    query = query.Where(c => c.Status == ContractStatus.ConHieuLuc && (c.EndDate == null || c.EndDate > DateTime.Now));
                else if (status == 2)
                {
                    var today = DateTime.Now;
                    var next30Days = today.AddDays(30);

                    query = query.Where(c => c.Status == ContractStatus.ConHieuLuc &&
                                             c.EndDate.HasValue &&
                                             c.EndDate >= today &&
                                             c.EndDate <= next30Days);
                }
                else if (status == 0)
                {
                    // Hoặc là status Expired, hoặc là ngày kết thúc đã qua
                    query = query.Where(c => c.Status == ContractStatus.HetHan ||
                                             (c.EndDate.HasValue && c.EndDate < DateTime.Now));
                }
            }
            if (contractType != -1)
            {
                query = query.Where(c => (int)c.Type == contractType);
            }
            // ✅ EF sẽ tự đóng connection sau mỗi truy vấn
            int total = await query.CountAsync();

            var data = await query
                .OrderByDescending(d => d.CreatedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();
            return (data, total);
        }
        public async Task CreateContractAsync(ContractRequest dto)
        {
            var con = new ContractModel
            {
                ContractCode = dto.ContractCode,
                EmployeeId = dto.EmployeeId,
                SignedDate = dto.SignedDate,
                StartDate = dto.StartDate,
                EndDate = dto.EndDate,
                BasicSalary = decimal.Parse(dto.BasicSalary),
                Type = dto.ContractType,
                Status = dto.status,
                Note = dto.Note,
                CreatedAt = DateTime.Now,
                FilePath = dto.FileUrl,
                IsActive = true,
                IsDeleted = false,
            };
            db.Contracts.Add(con);
            await db.SaveChangesAsync();
        }
        public async Task ExpirePreviousContractAsync(int employeeId, DateTime newStartDate)
        {
            var activeContract = await db.Contracts
                .Where(c => c.EmployeeId == employeeId
                            && c.Status == ContractStatus.ConHieuLuc
                            && !c.IsDeleted)
                .OrderByDescending(c => c.StartDate) 
                .FirstOrDefaultAsync();

            if (activeContract != null)
            {
                activeContract.Status = ContractStatus.HetHan;

                // Logic chốt ngày kết thúc như đã bàn
                if (activeContract.EndDate == null || activeContract.EndDate >= newStartDate)
                {
                    activeContract.EndDate = newStartDate.AddDays(-1);
                }
                db.Contracts.Update(activeContract);
                await db.SaveChangesAsync();
            }

        }
        public async Task RenewContractTransactionAsync(ContractRequest dto)
        {
            using var transaction = await db.Database.BeginTransactionAsync();
            try
            {
                await ExpirePreviousContractAsync(dto.EmployeeId, dto.StartDate);
                await CreateContractAsync(dto);
                await transaction.CommitAsync();
            }
            catch
            {
                await transaction.RollbackAsync();
                throw; // Ném lỗi ra để Controller bắt được và báo về Client
            }
        }
        public async Task UpdateContractAsync(int id, ContractRequest dto)
        {
            var con = await db.Contracts.FindAsync(id);
            if (con == null || con.IsDeleted)
                throw new ArgumentException("Contract not found");
            con.BasicSalary = decimal.Parse(dto.BasicSalary);
            con.StartDate = dto.StartDate;
            con.EndDate = dto.EndDate;
            con.SignedDate = dto.SignedDate;
            con.Type = dto.ContractType;
            con.Status = dto.status;
            con.Note = dto.Note;
            con.UpdatedAt = DateTime.Now;
            if (dto.FileContract != null && dto.FileContract.Length > 0)
            {
                con.FilePath = await EditCvAsync(dto.FileContract, dto.ExistingCvUrl);
            }
            else
            {
                con.FilePath = dto.ExistingCvUrl;
            }
            con.IsActive = true;
            con.IsDeleted = false;
            await db.SaveChangesAsync();
        }
        private async Task<string> EditCvAsync(IFormFile cvFile, string existingCvUrl)
        {
            if (cvFile == null || cvFile.Length == 0)
                return existingCvUrl; // Không upload file mới -> giữ file cũ

            // Xóa file cũ nếu có
            if (!string.IsNullOrEmpty(existingCvUrl))
            {
                var oldPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", existingCvUrl.TrimStart('/'));
                if (System.IO.File.Exists(oldPath))
                    System.IO.File.Delete(oldPath);
            }

            // Lưu file mới
            var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads", "cvs");
            if (!Directory.Exists(uploadsFolder))
                Directory.CreateDirectory(uploadsFolder);

            var fileName = Guid.NewGuid().ToString() + Path.GetExtension(cvFile.FileName);
            var filePath = Path.Combine(uploadsFolder, fileName);

            using (var fileStream = new FileStream(filePath, FileMode.Create))
            {
                await cvFile.CopyToAsync(fileStream);
            }

            return "/uploads/cvs/" + fileName; // trả về path mới để cập nhật DB
        }
        public async Task<string> GetNextContractCodeAsync()
        {
            var today = DateTime.Now;
            string yearShort = today.ToString("yy");
            string prefix = $"HĐ-{yearShort}-";
            var lastContract = await db.Contracts
                .Where(c => c.ContractCode.StartsWith(prefix))
                .OrderByDescending(c => c.ContractId)
                .FirstOrDefaultAsync();
            if (lastContract == null)
            {
                return prefix + "001";
            }
            string oldCode = lastContract.ContractCode;
            string numberPart = oldCode.Substring(prefix.Length);
            if (int.TryParse(numberPart, out int currentNumber))
            {
                int nextNumber = currentNumber + 1;
                return prefix + nextNumber.ToString("D3");
            }
            return prefix + "001";
        }
        public async Task<ContractSummary> GetContractSummaryAsync()
        {
            var now = DateTime.Now;
            var next30Days = now.AddDays(30);

            var query = db.Contracts.Where(c => !c.IsDeleted);

            var totalAll = await query.CountAsync();

            var totalActive = await query.CountAsync(c =>
                c.Status == ContractStatus.ConHieuLuc &&
                (c.EndDate == null || c.EndDate > now)
            );
            var totalNotYetEffective = await query.CountAsync(c =>
                c.Status == ContractStatus.ChuaHieuLuc 
            );


            var totalExpired = await query.CountAsync(c =>
                c.Status == ContractStatus.HetHan ||
                (c.EndDate.HasValue && c.EndDate < now)
            );

            return new ContractSummary
            {
                TotalAll = totalAll,
                TotalActive = totalActive,
                TotalNotYetEffective = totalNotYetEffective,
                TotalExpired = totalExpired
            };
        }

    }
}
