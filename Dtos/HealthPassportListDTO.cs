namespace PawsPort.Dtos
{
    // 1. 給 List 用的 DTO
    public class HealthPassportListDto
    {
        public int PassportId { get; set; }
        public int? PetId { get; set; }
        public string? Name { get; set; } // 關聯來的寵物名字
        public decimal? Weight { get; set; }
        public string? Note { get; set; }
        public int? RecordType { get; set; }
        public DateOnly? RecordDate { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public DateTime? CreatedAt { get; set; }
    }

    
    
}