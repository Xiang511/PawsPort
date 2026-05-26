namespace PawsPort.Dtos
{
    public class CommentListDTO
    {
        public int CommentId { get; set; }

        public int ArticleId { get; set; }

        public int UserId { get; set; }

        public string UserName { get; set; } = string.Empty;

        public string? UserPhoto { get; set; }

        public string Content { get; set; } = string.Empty;

        public DateTime CreateAt { get; set; }

        public DateTime? LastEditTime { get; set; }

        public string? ImageUrl { get; set; }

        public int? ParentId { get; set; }
    }
}
