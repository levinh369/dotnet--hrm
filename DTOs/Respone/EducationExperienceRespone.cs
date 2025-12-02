using static DACN.Enums.StatusEnums;

namespace DACN.DTOs.Respone
{
    public class EducationExperienceRespone
    {
        public int EducationExperienceId { get; set; }
        public EducationLevelEnum EducationLevel { get; set; }
        public string EducationLevelText => EducationLevel switch
        {
            EducationLevelEnum.CaoDang => "Cao đẳng",
            EducationLevelEnum.TrungCap => "Trung cấp",
            EducationLevelEnum.CuNhan => "Cử nhân",
            EducationLevelEnum.KySu => "Kỹ sư",
            EducationLevelEnum.ThacSi => "Thạc sĩ",
            EducationLevelEnum.TienSi => "Tiến sĩ",
            _ => "Không xác định"
        };
        public string? Major { get; set; }
        public string? University { get; set; }
        public int? GraduationYear { get; set; }
        public decimal? GPA { get; set; }
        public string? ExperienceDescription { get; set; }
    }
}
