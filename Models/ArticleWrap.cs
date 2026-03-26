using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace PawsPort.Models
{
    public class ArticleWrap
    {
        private Article _article; // 包裝的 Article 物件 //全域變數

        public Article article  // 包裝的 Article 物件的屬性
        {
            get { return _article; }
            set { _article = value; }
        }

        public ArticleWrap() // 建構子，初始化 Article 物件 //就像是出廠設定一樣
        {
            _article = new Article();
        }

        [DisplayName("文章ID")]
        public int ArticleId
        {
            get { return _article.ArticleId; }
            set { _article.ArticleId = value; }
        }

        [DisplayName("建立時間")]
        public DateTime CreateAt
        {
            get { return _article.CreateAt; }
            set { _article.CreateAt = value; }
        }
        [DisplayName("最後編輯時間")]
        public DateTime? LastEditTime
        {
            get { return _article.LastEditTime; }
            set { _article.LastEditTime = value; }
        }
        [DisplayName("標題")]
        [Required(ErrorMessage = "標題為必填欄位")]

        public string Title
        {
            get { return _article.Title; }
            set { _article.Title = value; }
        }
        [DisplayName("內容")]
        public string Content
        {
            get { return _article.Content; }
            set { _article.Content = value; }
        }
        [DisplayName("狀態")]
        public int Status
        {
            get { return _article.Status; }
            set { _article.Status = value; }
        }
        [DisplayName("瀏覽次數")]
        public int ViewCount
        {
            get { return _article.ViewCount; }
            set { _article.ViewCount = value; }
        }
        [DisplayName("檢舉次數")]
        public int ReportedCount
        {
            get { return _article.ReportedCount; }
            set { _article.ReportedCount = value; }
        }
        [DisplayName("最後檢舉時間")]
        public DateTime? LastReported
        {
            get { return _article.LastReported; }
            set { _article.LastReported = value; }
        }
        [DisplayName("活動開始日期")]
        public DateTime? EventStartDate
        {
            get { return _article.EventStartDate; }
            set { _article.EventStartDate = value; }
        }
        [DisplayName("活動結束日期")]
        public DateTime? EventEndDate
        {
            get { return _article.EventEndDate; }
            set { _article.EventEndDate = value; }
        }
        [DisplayName("活動地點")]
        public string? EventLocation
        {
            get { return _article.EventLocation; }
            set { _article.EventLocation = value; }
        }
        [DisplayName("是否存在")]
        public bool IsExist
        {
            get { return _article.IsExist; }
            set { _article.IsExist = value; }
        }
        [DisplayName("使用者ID")]
        public int UserId
        {
            get { return _article.UserId; }
            set { _article.UserId = value; }
        }
        [DisplayName("類別ID")]
        public int CategoryId
        {
            get { return _article.CategoryId; }
            set { _article.CategoryId = value; }
        }


    }
}
