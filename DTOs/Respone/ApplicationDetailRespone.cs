using Org.BouncyCastle.Bcpg.OpenPgp;
using static DACN.Enums.StatusEnums;

namespace DACN.DTOs.Respone
{
    public class ApplicationDetailRespone
    {
        public string? JobTitle { get; set; }  
        public DateTime? SubmitAt { get; set; }
        public string? SalaryRange { get; set; }
        public string? CvUrl { get; set; }
        public string? CvFileName { get; set; }
        public string? CandidateName { get; set; }
        public string? CandidateEmail { get; set; }
        public string? CandidatePhone { get; set; }
        public int ApplicationId { get; set; }
        public ApplicationStatus Status { get; set; }
    }
}
