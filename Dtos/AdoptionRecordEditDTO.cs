namespace PawsPort.DTOs
{
   
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