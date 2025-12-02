using DACN.Models;
using System.ComponentModel.DataAnnotations;
using static DACN.Enums.StatusEnums;

public class JobApplicationModel
{
    [Key]
    public int Id { get; set; }

    // FK nếu nhân viên đã được approve
    public int? EmployeeId { get; set; }
    public EmployeeModel? Employee { get; set; }

    // FK luôn tồn tại, liên kết với UserAccount
    [Required]
    public int UserAccountId { get; set; }
    public UserAccountModel UserAccount { get; set; }

    [Required]
    public int JobPostingId { get; set; }
    public JobPostingModel? JobPosting { get; set; }


    [StringLength(255)]
    public string? Skills { get; set; }

    [StringLength(255)]
    public string? CvFilePath { get; set; }

    public DateTime SubmittedDate { get; set; } = DateTime.Now;

    public ApplicationStatus Status { get; set; } = ApplicationStatus.Pending;

    [StringLength(255)]
    public string? Notes { get; set; }

    public DateTime? ReviewedDate { get; set; }
    public EmploymentType AppliedType { get; set; }
    public bool IsDeleted { get; set; } = false;
}
