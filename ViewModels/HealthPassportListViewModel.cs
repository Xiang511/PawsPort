namespace PawsPort.ViewModels
{
    public class HealthPassportListViewModel
    {
        public int PassportId { get; set; }
        public int? PetId { get; set; }

        // 👇 這是我們特別從 Pet 資料表借過來的新欄位！
        public string? Name { get; set; }

        public decimal? Weight { get; set; } // 
        public string? Note { get; set; }
        public int? RecordType { get; set; }
        public DateOnly? RecordDate { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public DateTime? CreatedAt { get; set; }
    }
}
