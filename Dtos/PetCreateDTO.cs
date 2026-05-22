namespace PawsPort.Dtos
{
   

    // 2. 新增用
    public class PetCreateDto
    {
        public int? Species { get; set; }
        public string? Name { get; set; }
        public int? Gender { get; set; }
        public int? Size { get; set; }
        public string? CoatColor { get; set; }
        public DateOnly? BirthDate { get; set; }
        public string? Photo { get; set; }
        public int? CurrentStatus { get; set; }
        public string? BehavioralTraits { get; set; }
        public bool? IsHighMaintenance { get; set; }
        public string? Note { get; set; }
        public bool? IsDesex { get; set; }
        public string? Microchip { get; set; }
    }

   
}