namespace PawsPort.DTOs
{
    // 1. 給 List 用的 DTO (包含關聯出來的寵物名字 Name)
    public class AdoptionRecordListDto
    {
        public int AdoptionId { get; set; }
        public string? Name { get; set; } // 從 Pet 表 Join 來的
        public int? PetId { get; set; }
        public int? UserId { get; set; }
        public DateOnly? ApplyDate { get; set; }
        public DateOnly? AdoptDate { get; set; }
        public DateOnly? ReturnDate { get; set; }
        public string? ReturnReason { get; set; }
        public DateOnly? FollowUpDeadline { get; set; }
        public int? Status { get; set; }
    }

    // 2. 給 Create 用的 DTO (不需要 AdoptionId 和 Name)
    public class AdoptionRecordCreateDto
    {
        public int? PetId { get; set; }
        public int? UserId { get; set; }
        public DateOnly? ApplyDate { get; set; }
        public DateOnly? AdoptDate { get; set; }
        public int? Status { get; set; }
    }

    // 3. 給 Edit 用的 DTO (必須要有 AdoptionId，用來找舊資料)
    public class AdoptionRecordEditDto
    {
        public int AdoptionId { get; set; }
        public int? UserId { get; set; }
        public DateOnly? ApplyDate { get; set; }
        public DateOnly? AdoptDate { get; set; }
        public DateOnly? ReturnDate { get; set; }
        public string? ReturnReason { get; set; }
        public DateOnly? FollowUpDeadline { get; set; }
        public int? Status { get; set; }
    }
}