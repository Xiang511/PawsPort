namespace PawsPort.Dtos
{
    public class CommentSaveDTO
    {

        public string Content { get; set; }

        public string? ImageUrl { get; set; }

        public int UserId { get; set; }

        public int ArticleId { get; set; }

        public int? ParentId { get; set; }

        //public int Status { get; set; }

        //public List<IFormFile>? ImageFiles { get; set; }
    }
}
