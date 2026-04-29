namespace PawsPort.Dtos
{
    // 1. 給 List 用的 DTO
   

    // 2. 給 Create 用的 DTO (精簡掉不需要的 ID 欄位)
    public class HealthPassportCreateDto
    {
        public int? PetId { get; set; }
        public decimal? Weight { get; set; }
        public string? Note { get; set; }
        public int? RecordType { get; set; }
        public DateOnly? RecordDate { get; set; }

        // 病歷資料
        public string? TreatmentLocation { get; set; }
        public string? Disease { get; set; }
        public string? DiseaseTreatment { get; set; }
        public DateTime? TreatmentTime { get; set; }
        public DateTime? TreatmentUpdatedAt { get; set; }

        public DateTime? TreatmentCreatedAt { get; set; }

        // 疫苗資料
        public string? Type { get; set; }
        public string? VaccinationLocation { get; set; }
        public DateTime? VaccinationTime { get; set; }
        public DateOnly? Forecast { get; set; }
        public DateTime? VaccinationUpdatedAt { get; set; }

        public DateTime? VaccinationCreatedAt { get; set; }
    }

    
    
}