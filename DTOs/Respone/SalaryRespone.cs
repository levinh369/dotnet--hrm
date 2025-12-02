using Microsoft.EntityFrameworkCore.Storage;
using static DACN.Enums.StatusEnums;

namespace DACN.DTOs.Respone
{
    public class SalaryRespone
    {
        public int SalaryId { get; set; }
        public int EmployeeId { get; set; }
        public string? EmployeeName { get; set; }
        public decimal BaseSalary { get; set; }
        public decimal Allowance { get; set; }
        public decimal TotalSalary { get; set; }
        public string? DepartmentName { get; set; }
        public string? PositionName { get; set; }
        public string? Avatar { get; set; }
        public int WorkDays { get; set; }
        public decimal Deduction { get; set; }
        public decimal NetSalary { get; set; }
        public SalaryStatus SalaryStatus { get; set; }
        public string? Note { get; set; }
        public decimal Bonus { get; set; }
        public decimal ManualDeduction { get; set; }
        public int Month { get; set; }
        public int Year { get; set; }
        public float StandardWorkDays { get; set; }

    }
}
