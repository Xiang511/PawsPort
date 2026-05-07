namespace PawsPort.Dtos
{
    public class ENewsletterUpdateDTO
    {
        public string Title { get; set; }
        public string Summary { get; set; }
        public string Content { get; set; }
        public string Category { get; set; }
        public string Status { get; set; }
        public string Note { get; set; }
        public DateTime? PublishDate { get; set; }
        public string? Image { get; set; }
    }
}
