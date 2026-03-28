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

        [DisplayName("人氣指數")]
        public int PopularityScore => (FollowerCount * 2) + ReceivedCommentCount; 
        //人氣指數計算方式：粉絲數量*2 + 收到的留言數量

    }
}
