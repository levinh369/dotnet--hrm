using DACN.Models;
using System.ComponentModel.DataAnnotations;
using static DACN.Enums.StatusEnums;

public class EmployeeModel
{
    [Key]
    public int EmployeeId { get; set; }
    public DateTime? DateOfBirth { get; set; }

    public bool Gender { get; set; }
    [StringLength(255)]
    public string? Address { get; set; }
    [StringLength(20)]
    public string? Phone { get; set; }
    [StringLength(20)]
    public string? CCCD { get; set; }

    public DateTime? StartDate { get; set; }
    public EmployeeStatus Status { get; set; }

    public int? DepartmentId { get; set; }
    public DepartmentModel? Department { get; set; }

    public int? PositionId { get; set; }
    public PositionModel? Position { get; set; }

    public ICollection<ContractModel>? Contracts { get; set; }

    public int UserAccountId { get; set; }        // FK duy nhất
    public UserAccountModel Account { get; set; } // navigation

    public DateTime CreatedAt { get; set; } = DateTime.Now;
    public DateTime? UpdatedAt { get; set; }
    public bool IsDeleted { get; set; } = false;
    public string? AvatarUrl { get; set; }
    public bool IsActive { get; set; }
}
