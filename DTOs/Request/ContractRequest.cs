using static DACN.Enums.StatusEnums;

namespace DACN.DTOs.Request
{
    public class ContractRequest
    {
        public string ContractCode { get; set; }
        public int EmployeeId { get; set; }
        public ContractType ContractType { get; set; }
        public string BasicSalary { get; set; } 
        public DateTime StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public DateTime SignedDate { get; set; }
        public IFormFile? FileContract { get; set; }
        public string? ExistingCvUrl { get; set; }  
        public string? FileUrl { get; set; }
        public string? Note { get; set; }
        public ContractStatus status { get; set; }
    }
}
