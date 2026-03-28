namespace PawsPort.ViewModels
{
    public class SocialDetailsViewModel
    {
        public int UserId { get; set; }

        public string UserName { get; set; }

        public int PostCount { get; set; }

        public int FollowerCount { get; set; }

        public int FollowingCount { get; set; }

        public int MyCommentCount { get; set; }
        public int ReceivedCommentCount { get; set; }

        public int MyBookmarkCount { get; set; }
        public int ReceivedBookmarkCount { get; set; } 

        public List<ArticleSummary> RecentArticles { get; set; } = new List<ArticleSummary>();
        //最近的發文紀錄
    }

    public class ArticleSummary
    {
        public int ArticleId { get; set; }

        public string Title { get; set; }

        public DateTime CreateAt { get; set; }

        public int ViewCount { get; set; }
    }
}
