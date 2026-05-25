using System;

namespace PawsPort.Dtos
{
    public class CreateOrUpdatePassportDTO
    {
        // Discriminator to decide which sub‑entity to handle
        public string DetailType { get; set; }   // "medical" | "vaccine" | "image" | "weight"

        public int UserId { get; set; }

        // Common optional field – if omitted the service will resolve passport from JWT
        public int? PassportId { get; set; }

        // MedicalHistory fields (used when DetailType == "medical")
        public string? Location { get; set; }
        public string? Disease { get; set; }
        public string? DiseaseTreatment { get; set; }
        public DateTime? Time { get; set; }

        // VaccinationStatus fields (used when DetailType == "vaccine")
        public string? VaccineType { get; set; }
        public string? VaccineLocation { get; set; }
        public DateTime? VaccineTime { get; set; }
        public DateOnly? Forecast { get; set; }

        // Image fields (used when DetailType == "image")
        public string? PhotoBase64 { get; set; }   // Base64‑encoded image data
        public string? PhotoNote { get; set; }

        // Weight fields (used when DetailType == "weight") – stored in HealthPassport
        public decimal? Weight { get; set; }
        public DateOnly? RecordDate { get; set; }
        public string? Note { get; set; }
    }
}
