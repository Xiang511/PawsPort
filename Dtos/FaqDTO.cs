namespace PawsPort.Dtos
{
    public class FaqDTO
    {
        public int Faqid { get; set; }
        public string QuestionType { get; set; }
        public string Question { get; set; }
        public string Answer { get; set; }
        public DateTime? CreateAt { get; set; }
        public string Status { get; set; }



    }
}
