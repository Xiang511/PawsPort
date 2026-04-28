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

    // 2. 新增用
    public class MissingReportCreateDto
    {
        public int? PetId { get; set; }
        public DateTime? LastSeenDate { get; set; }
        public bool? IsActive { get; set; }
        public decimal? LastSeenLat { get; set; }
        public decimal? LastSeenLng { get; set; }
        public string? LostLocation { get; set; }
        public int? UserId { get; set; }
    }

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