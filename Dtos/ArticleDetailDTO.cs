namespace PawsPort.Dtos
{
    public class ArticleDetailDTO
    {
        public int ArticleId { get; set; }

        public DateTime CreateAt { get; set; }

        public DateTime? LastEditTime { get; set; }

        public string Title { get; set; }

        public string Content { get; set; }

        public int Status { get; set; }

        public int ViewCount { get; set; }

        public int ReportedCount { get; set; }

        public DateTime? LastReported { get; set; }

        public DateTime? EventStartDate { get; set; }

        public DateTime? EventEndDate { get; set; }

        public string EventLocation { get; set; }

        public bool IsExist { get; set; }

        public int UserId { get; set; }

        public int CategoryId { get; set; }

        public int? DeleteTypeId { get; set; }

        public string DeleteNote { get; set; }

        public bool IsActive { get; set; }

        //
        public string CategoryName { get; set; } = string.Empty;

        public string UserName { get; set; } = string.Empty;

        public string? UserPhoto { get; set; }

        public List<string> Tags { get; set; } = new();

        public int BookmarkCount { get; set; }

        public int LikeCount { get; set; }

        public int CommentCount { get; set; }

        public bool IsBookmarked { get; set; }

        public bool IsLiked { get; set; }

        public bool IsFollowingAuthor { get; set; }

        public List<ArticleCommentDTO> Comments { get; set; } = new();
        public string? CoverImageUrl { get; set; }

        public List<string> ImageUrls { get; set; } = new();

    }
}
