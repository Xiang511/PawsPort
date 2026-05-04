namespace PawsPort.Dtos
{
    public class QaDTO
    {
        public int Qaid { get; set; }
        public string Csname { get; set; }
        public string ReplyContent { get; set; }
        public string Note { get; set; }
        public DateTime? ReplyDate { get; set; }   // 可能有空值，所以加?

    }
}
