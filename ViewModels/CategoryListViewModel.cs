namespace PawsPort.ViewModels
{
    public class CategoryListViewModel
    {
        public string? TxtKeyword { get; set; } //搜尋名稱關鍵字

        public string? SortBy { get; set; } = "CreateAt"; //按照新增時間排序
        public string? SortOrder { get; set; } = "desc"; //由新到舊

        // 分頁控制
        public int CurrentPage { get; set; } = 1;
        public int PageSize { get; set; } = 10;
        public int TotalCount { get; set; }
        public int TotalPages => (int)Math.Ceiling((double)TotalCount / PageSize);

        // 資料列表 (只包含父分類，子分類會掛在父分類下面)
        public List<CategoryItemViewModel> ParentCategories { get; set; }

        public bool IsExpanded { get; set; } = false; //預設手風琴全部展開

    }

    public class CategoryItemViewModel
    {
        public int CategoryId { get; set; }
        public string CategoryName { get; set; }
        public string CategoryDescription { get; set; }
        public int? ParentID { get; set; }
        public int Level { get; set; }
        public int SortOrder { get; set; }
        public DateTime? CreateAt { get; set; }
        public int ArticleCount { get; set; }
        public string ColorClass { get; set; } // 用來存 Bootstrap 顏色類別 (如 primary, success)

        // 用來存放該父分類下的所有子分類
        public List<CategoryItemViewModel> ChildCategories { get; set; } = new List<CategoryItemViewModel>();
    }
}
