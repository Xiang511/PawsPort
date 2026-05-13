namespace PawsPort.Dtos
{
    // 1. 列表用 (包含基礎資訊)
    public class PetListDto
    {
        public int PetId { get; set; }
        public string? Name { get; set; }
        public string? CoatColor { get; set; }
        public int? Gender { get; set; }
        public int? Size { get; set; }
        public int? CurrentStatus { get; set; }
        public DateTime? CreatedAt { get; set; }
        public int? Species { get; set; }
        public DateOnly? BirthDate { get; set; }
        public string? Photo { get; set; }
        public string? BehavioralTraits { get; set; }
        public bool? IsHighMaintenance { get; set; }
        public string? Note { get; set; }
        public bool? IsDesex { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public DateTime? DeletedAt { get; set; }
        public string? Microchip { get; set; }
        
    }

   
}