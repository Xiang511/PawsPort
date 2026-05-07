namespace PawsPort.DTOs
{
    

    // 2. 給 Create 用的 DTO (不需要 AdoptionId 和 Name)
    public class AdoptionRecordCreateDto
    {
        public int? PetId { get; set; }
        public int? UserId { get; set; }
        public DateOnly? ApplyDate { get; set; }
        public DateOnly? AdoptDate { get; set; }
        public int? Status { get; set; }
    }

    
}