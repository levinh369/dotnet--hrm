using DACN.Data;
using DACN.DTOs.Request;
using DACN.DTOs.Respone;
using DACN.Models;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.EntityFrameworkCore;
using static DACN.Enums.StatusEnums;

namespace DACN.Repositories
{
    public class SalaryRepository
    {
        private readonly ApplicationDbContext db;

        public SalaryRepository(ApplicationDbContext db)
        {
            this.db = db;
        }
        public async Task<(List<SalaryModel> Data, int Total)> GetPagedAsync(
       int page, int pageSize, int month, int year)
        {
            var query = db.Salaries
             .Include(a => a.Employee)
                .ThenInclude(e => e.Account)
             .Include(a => a.Employee)
                .ThenInclude(e => e.Department)
             .AsNoTracking()
             .AsQueryable();
            if (month > 0 && year > 0)
                query = query.Where(m => m.Month == month && m.Year == year);
            int total = await query.CountAsync();

            var data = await query
               .OrderBy(d => d.Employee.Account.FullName)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return (data, total);
        }
        public async Task<(bool Success, string Message)> CalculateAndSavePayrollAsync(int month, int year)
        {
            try
            {
                var activeEmployees = await db.Employees
                    .Include(e => e.Contracts)
                    .Where(e => e.Status == EmployeeStatus.DangLam && e.Contracts != null)
                    .ToListAsync();

                if (!activeEmployees.Any())
                    return (false, "Không tìm thấy nhân viên nào có hợp đồng để tính lương.");

                var startDate = new DateTime(year, month, 1);
                var endDate = startDate.AddMonths(1).AddDays(-1);

                var attendanceData = await db.Attendances
                    .Where(a => a.WorkDate >= startDate && a.WorkDate <= endDate && a.CheckInTime != null)
                    .Select(a => new
                    {
                        a.EmployeeId,
                        LateMinutes = (a.Status == AttendanceStatus.DiMuon && a.CheckInTime.Value > new TimeSpan(8, 0, 0))
                                      ? (a.CheckInTime.Value - new TimeSpan(8, 0, 0)).TotalMinutes
                                      : 0
                    })
                    .ToListAsync();

                int countCreated = 0;
                int countUpdated = 0;

                // 3. CẤU HÌNH
                double standardWorkDays = 26.0;
                decimal penaltyPerMinute = 1000;
                decimal insuranceRate = 0.105m;
                foreach (var emp in activeEmployees)
                {
                    var empAtt = attendanceData.Where(x => x.EmployeeId == emp.EmployeeId).ToList();
                    double actualWorkDays = empAtt.Count;
                    double totalLateMinutes = empAtt.Sum(x => x.LateMinutes);

                    var contract = emp.Contracts
                                    ?.OrderByDescending(c => c.StartDate)
                                    .FirstOrDefault();

                    decimal baseSalary = contract?.BasicSalary ?? 0;
                    decimal allowance = contract?.Allowance ?? 0;
                    decimal salaryByWorkDay = 0;
                    if (standardWorkDays > 0)
                    {
                        salaryByWorkDay = (baseSalary / (decimal)standardWorkDays) * (decimal)actualWorkDays;
                    }

                    decimal insuranceDed = 0;
                    if (actualWorkDays >= 14) 
                    {
                        insuranceDed = baseSalary * insuranceRate;
                    }

                    // 3. Tính Phạt đi muộn
                    decimal lateDed = (decimal)totalLateMinutes * penaltyPerMinute;

                    // Tổng Khấu Trừ
                    decimal totalAutoDeduction = insuranceDed + lateDed;
                    var salaryRow = await db.Salaries
                        .FirstOrDefaultAsync(s => s.EmployeeId == emp.EmployeeId && s.Month == month && s.Year == year);

                    if (salaryRow != null)
                    {
                        if (salaryRow.Status != 0) continue; 

                        salaryRow.StandardWorkDays = standardWorkDays;
                        salaryRow.ActualWorkDays = actualWorkDays;
                        salaryRow.BaseSalary = baseSalary;
                        salaryRow.Allowance = allowance;
                        salaryRow.Deduction = totalAutoDeduction;
                        salaryRow.NetSalary = salaryByWorkDay + allowance - totalAutoDeduction;

                        salaryRow.UpdatedAt = DateTime.Now;
                        db.Salaries.Update(salaryRow);
                        countUpdated++;
                    }
                    else
                    {
                        // INSERT
                        var newSalary = new SalaryModel
                        {
                            EmployeeId = emp.EmployeeId,
                            Month = month,
                            Year = year,
                            StandardWorkDays = standardWorkDays,
                            ActualWorkDays = actualWorkDays,
                            BaseSalary = baseSalary,
                            Allowance = allowance,
                            Deduction = totalAutoDeduction,
                            NetSalary = salaryByWorkDay + allowance - totalAutoDeduction,
                            Status = 0,
                            CreatedAt = DateTime.Now,
                        };
                        db.Salaries.Add(newSalary);
                        countCreated++;
                    }
                }

                await db.SaveChangesAsync();
                return (true, $"Đã tính xong! Thêm mới: {countCreated}, Cập nhật: {countUpdated}");
            }
            catch (Exception ex)
            {
                return (false, "Lỗi hệ thống: " + ex.Message);
            }
        }
        public async Task<SalaryRespone?> GetDetailSalary(int salaryId)
        {
            var salaryDetail = await db.Salaries
                 .Include(s => s.Employee)
                     .ThenInclude(e => e.Account)
                 .Include(s => s.Employee)
                     .ThenInclude(e => e.Department)
                 .Include(s => s.Employee)
                     .ThenInclude(e => e.Position)
                 .AsNoTracking() // Tối ưu hiệu năng vì chỉ lấy ra xem
                .FirstOrDefaultAsync(s => s.SalaryId == salaryId);
            var response = salaryDetail == null ? null : new SalaryRespone
            {
                SalaryId = salaryDetail.SalaryId,
                EmployeeId = salaryDetail.EmployeeId,
                EmployeeName = salaryDetail.Employee.Account.FullName,
                DepartmentName = salaryDetail.Employee.Department.DepartmentName,
                PositionName = salaryDetail.Employee.Position.PositionName,
                StandardWorkDays = (int)salaryDetail.StandardWorkDays,
                BaseSalary = salaryDetail.BaseSalary,
                Allowance = salaryDetail.Allowance,
                Bonus = salaryDetail.Bonus,
                ManualDeduction = salaryDetail.ManualDeduction,
                Deduction = salaryDetail.Deduction,
                NetSalary = salaryDetail.NetSalary,
                WorkDays = (int)salaryDetail.ActualWorkDays,
                Avatar = salaryDetail.Employee.AvatarUrl,
                SalaryStatus = salaryDetail.Status,
                Month = salaryDetail.Month,
                Year = salaryDetail.Year,
                Note = salaryDetail.Note
            };
            return response;
        }
        public async Task UpdateSalaryById(int salaryId, SalaryRequest dto, string userUpdating)
        {
            var salary = await db.Salaries.FindAsync(salaryId);
            if (salary == null)
                throw new Exception("Không tìm thấy bản ghi lương.");

            if (salary.Status != SalaryStatus.Draft)
                throw new Exception("Bảng lương này đã chốt hoặc đã thanh toán, không thể chỉnh sửa.");

            salary.ManualDeduction = dto.ManualDeduction;
            salary.Bonus = dto.Bonus;
            salary.Note = dto.Note;
            salary.UpdatedAt = DateTime.Now;
            salary.UpdatedBy = userUpdating;
            decimal salaryByWorkDay = 0;
            if (salary.StandardWorkDays > 0)
            {
                salaryByWorkDay = (salary.BaseSalary / (decimal)salary.StandardWorkDays) * (decimal)salary.ActualWorkDays;
            }
            salary.NetSalary = salaryByWorkDay
                       + salary.Allowance
                       + salary.Bonus           
                       - salary.Deduction      
                       - salary.ManualDeduction;
            await db.SaveChangesAsync();
        }
    }
}
