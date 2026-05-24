using System;
using System.Collections.Generic;

namespace PawsPort.Dtos
{
    public class UnifiedPassportDTO
    {
        public int PassportId { get; set; }
        public int? PetId { get; set; }
        public string PetName { get; set; }
        public string Gender { get; set; }
        public string Age { get; set; }
        public decimal? Weight { get; set; }
        public DateOnly? RecordDate { get; set; }
        public string Note { get; set; }
        public string PhotoBase64 { get; set; }
        public List<MedicalDto> MedicalHistories { get; set; }
        public List<VaccineDto> VaccinationStatuses { get; set; }
    }

    public class MedicalDto
    {
        public int MedicalDetailId { get; set; }
        public string Location { get; set; }
        public string Disease { get; set; }
        public string DiseaseTreatment { get; set; }
        public DateTime? Time { get; set; }
    }

    public class VaccineDto
    {
        public int HistoryId { get; set; }
        public string Type { get; set; }
        public string Location { get; set; }
        public DateTime? Time { get; set; }
        public DateOnly? Forecast { get; set; }
    }
}
