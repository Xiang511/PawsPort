using System.ComponentModel.DataAnnotations;

namespace PawsPort.Dtos
{
    public class UpdateArticleDTO
    {
        //隱藏
        public int ArticleId { get; set; }

        public int UserId { get; set; }

        public int CategoryId { get; set; }

        //條件顯示
        public DateTime? EventStartDate { get; set; }

        public DateTime? EventEndDate { get; set; }

        public string EventLocation { get; set; }

        //顯示
        [Required]
        [StringLength(100, ErrorMessage = "標題長度不能超過100個字元")]
        public string Title { get; set; }

        public string Content { get; set; }

        [Required]
        [Range(0, 2)] //0:草稿, 1:公開, 2:私人
        public int Status { get; set; }

        public string UserName { get; set; }

        public string CategoryName { get; set; }

        public List<string> TagNames { get; set; }

        public bool IsExist { get; set; }

        //public List<IFormFile>? ImageFiles { get; set; }
    }
}
