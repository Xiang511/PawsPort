namespace PawsPort.Dtos
{
    
    // 3. 修改用
    public class MissingReportEditDto
    {
        public int ReportId { get; set; }
        public DateTime? LastSeenDate { get; set; }
        public decimal? LastSeenLat { get; set; }
        public decimal? LastSeenLng { get; set; }
        public bool? IsActive { get; set; }
        public string? LostLocation { get; set; }
    }
}