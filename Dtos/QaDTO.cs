namespace PawsPort.Dtos
{
    public class QaDTO
    {
        public int Qaid { get; set; }
        public int UserId { get; set; }
        public string QuestionType { get; set; }
        public string ChiefComplaint { get; set; }
        public string Csname { get; set; }
        public DateTime QuestionDate { get; set; }
        public string ReplyContent { get; set; }
        public string Note { get; set; }
        public DateTime? ReplyDate { get; set; }   // 可能有空值，所以加?
        public int? Score { get; set; }

    }
}
