namespace PawsPort.Dtos
{
    public class ArticleCommentDTO
    {
        public int CommentId { get; set; }

        public int UserId { get; set; }

        public string UserName { get; set; } = string.Empty;

        public string? UserPhoto { get; set; }

        public string Content { get; set; } = string.Empty;

        public DateTime CreateAt { get; set; }

        public int FloorNumber { get; set; }

        public int LikeCount { get; set; }

    }
}
