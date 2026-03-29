using PawsPort.Models;

namespace PawsPort.ViewModels
{
    public class ArticleEditViewModel
    {
        
            // 文章基本資訊
            public int ArticleId { get; set; }
            public string Title { get; set; }
            public string Content { get; set; }
            public int CategoryId { get; set; }
            public string? ExistingImageName { get; set; } // 現有的圖片檔名

            // 下拉選單資料 (由 Controller 填充)
            public List<Category> CategoryList { get; set; }

            // 接收上傳的檔案
            public IFormFile? UploadImage { get; set; }
        
    }
}

