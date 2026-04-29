namespace PawsPort.Dtos
{
    // 1. 列表用 (包含寵物名字)
    public class MissingReportListDto
    {
        public int ReportId { get; set; }
        public int? PetId { get; set; }
        public string? Name { get; set; }
        public DateTime? LastSeenDate { get; set; }
        public bool? IsActive { get; set; }
        public decimal? LastSeenLat { get; set; }
        public decimal? LastSeenLng { get; set; }
        public string? LostLocation { get; set; }
        public int? UserId { get; set; }
        public DateTime? CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }

   
}