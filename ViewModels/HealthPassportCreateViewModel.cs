namespace PawsPort.ViewModels
{
    public class HealthPassportCreateViewModel
    {
        public int PassportId { get; set; }
        public int? PetId { get; set; }

        // 從 Pet 資料表借過來的新欄位！
        public string? Name { get; set; }

        public decimal? Weight { get; set; } // 
        public string? Note { get; set; }
        public int? RecordType { get; set; }
        public DateOnly? RecordDate { get; set; }
        public DateTime? CreatedAt { get; set; }
        public int? MedicalDetailId { get; set; }

        public string? TreatmentLocation { get; set; }

        public string? Disease { get; set; }

        public string? DiseaseTreatment { get; set; }

        public DateTime? TreatmentTime { get; set; }

        public DateTime? TreatmentCreatedAt { get; set; }

        public int? HistoryId { get; set; }

        public string? Type { get; set; }

        public string? VaccinationLocation { get; set; }

        public DateTime? VaccinationTime { get; set; }

        public DateOnly? Forecast { get; set; }

        public DateTime? VaccinationCreatedAt { get; set; }
    }
}
