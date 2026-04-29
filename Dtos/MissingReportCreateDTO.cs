namespace PawsPort.Dtos
{
    

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

    
}