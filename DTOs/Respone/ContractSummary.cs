namespace DACN.DTOs.Respone
{
    public class ContractSummary
    {
        public int TotalAll { get; set; }
        public int TotalActive { get; set; }
        public int TotalNotYetEffective { get; set; }
        public int TotalExpired { get; set; }
    }
}
