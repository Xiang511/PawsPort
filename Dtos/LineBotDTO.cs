namespace PawsPort.Dtos
{
    public class LineBotDTO
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public DateTime ChatDate { get; set; }
        public string QuestionType { get; set; }
        public string ChiefComplaint { get; set; }
        public string ChatContent { get; set; }
    }
}
