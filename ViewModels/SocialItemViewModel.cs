using System.ComponentModel;

namespace PawsPort.ViewModels
{
    public class SocialItemViewModel
    {
        [DisplayName("會員編號")]
        public int UserId { get; set; }

        [DisplayName("會員名稱")]
        public string UserName { get; set; }

        [DisplayName("粉絲")]
        public int FollowerCount { get; set; }//追蹤者數量

        [DisplayName("追蹤")]
        public int FollowingCount { get; set; } //追蹤的使用者數量

        [DisplayName("發文數")]
        public int PostCount { get; set; } //發文數量

        [DisplayName("留言")]
        public int MyCommentCount { get; set; }//我去留言的數量

        [DisplayName("收到留言")]
        public int ReceivedCommentCount { get; set; } //我收到的留言數量

        [DisplayName("收藏他人")]
        public int MyBookmarkCount { get; set; } //我的書籤數量
        
        [DisplayName("被收藏數")]
        public int ReceivedBookmarkCount { get; set; } //我收到的書籤數量

        [DisplayName("貼文總閱覽次數")]
        public int TotalViewCount { get; set; } //貼文被閱覽次數

        [DisplayName("人氣程度")]
       public int PopularityScore { get; set; } //加權算法在controller

      

    }
}
