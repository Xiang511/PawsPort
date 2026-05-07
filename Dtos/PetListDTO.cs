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
    }

   
}