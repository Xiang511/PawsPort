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

    // 2. 新增用
    public class PetCreateDto
    {
        public int? SpeciesId { get; set; }
        public string? Name { get; set; }
        public int? Gender { get; set; }
        public int? Size { get; set; }
        public string? CoatColor { get; set; }
        public int? CurrentStatus { get; set; }
        public string? BehavioralTraits { get; set; }
        public bool? IsHighMaintenance { get; set; }
        public string? Note { get; set; }
        public bool? IsDesex { get; set; }
    }

    // 3. 修改用 (包含 ID)
    public class PetEditDto
    {
        public int PetId { get; set; }
        public int? SpeciesId { get; set; }
        public string? Name { get; set; }
        public int? Gender { get; set; }
        public int? Size { get; set; }
        public string? CoatColor { get; set; }
        public int? CurrentStatus { get; set; }
        public string? BehavioralTraits { get; set; }
        public bool? IsHighMaintenance { get; set; }
        public string? Note { get; set; }
        public bool? IsDesex { get; set; }
    }
}