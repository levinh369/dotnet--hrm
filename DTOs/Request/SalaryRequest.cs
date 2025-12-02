namespace DACN.DTOs.Request
{
    public class SalaryRequest
    {
        public decimal BaseSalary { get; set; }
        public decimal Bonus { get; set; }
        public decimal ManualDeduction { get; set; }
        public string? Note { get; set; }

    }
}
