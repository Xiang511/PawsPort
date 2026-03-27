namespace PawsPort.ViewModels
{
    public class HealthPassportDetailsViewModel
    {
        public int? MedicalDetailId { get; set; }

        public string? TreatmentLocation { get; set; }

        public string? Disease { get; set; }

        public string? DiseaseTreatment { get; set; }

        public DateTime? TreatmentTime { get; set; }

        public int? PassportId { get; set; }

        public DateTime? TreatmentUpdatedAt { get; set; }

        public DateTime? TreatmentCreatedAt { get; set; }

        public int? HistoryId { get; set; }

        public string? Type { get; set; }

        public string? VaccinationLocation { get; set; }

        public DateTime? VaccinationTime { get; set; }

        public DateOnly? Forecast { get; set; }

        

        public DateTime? VaccinationUpdatedAt { get; set; }

        public DateTime? VaccinationCreatedAt { get; set; }
    }
}
