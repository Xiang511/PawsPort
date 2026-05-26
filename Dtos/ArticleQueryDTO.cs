namespace PawsPort.Dtos
{
    public class ArticleQueryDTO
    {
        public int? Status { get; set; }
        public bool? IsActive { get; set; }
        public int? UserId { get; set; }

        public string? Keyword { get; set; }
        public string? Tag { get; set; }
    }
}
