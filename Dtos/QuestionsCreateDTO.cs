namespace PawsPort.Dtos
{
    public class QuestionsCreateDTO
    {
        public string GameName { get; set; }
        public string Questions { get; set; }
        public string AnswersDetail { get; set; }
        public int Answers { get; set; }
        public bool IsActive { get; set; }
        public int Rewards { get; set; }
        public string Type { get; set; }
    }
}