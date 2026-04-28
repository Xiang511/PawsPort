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

    // 3. 給 Edit 用的 DTO (只保留允許被修改的欄位)
    public class HealthPassportEditDto
    {
        public int PassportId { get; set; }
        public decimal? Weight { get; set; }
        public string? Note { get; set; }
        public int? RecordType { get; set; }
    }

    // 4. 給 Details 用的 DTO
    public class HealthPassportDetailsDto
    {
        public int PassportId { get; set; }

        // 病歷
        public int? MedicalDetailId { get; set; }
        public string? TreatmentLocation { get; set; }
        public string? Disease { get; set; }
        public string? DiseaseTreatment { get; set; }
        public DateTime? TreatmentTime { get; set; }
        public DateTime? TreatmentUpdatedAt { get; set; }

        public DateTime? TreatmentCreatedAt { get; set; }

        // 疫苗
        public int? HistoryId { get; set; }
        public string? Type { get; set; }
        public string? VaccinationLocation { get; set; }
        public DateTime? VaccinationTime { get; set; }
        public DateTime? VaccinationUpdatedAt { get; set; }

        public DateTime? VaccinationCreatedAt { get; set; }
    }
}