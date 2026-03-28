namespace PawsPort.ViewModels
{
    public class SocialListViewModel
    {
        public string? TxtKeyword { get; set; } //搜尋使用者名稱或文章內容的關鍵字

        public int CurrentPage { get; set; } = 1;//目前頁數

        public int PageSize { get; set; }=10; //預設10筆
        //每頁顯示的資料數量
        public int TotalCount { get; set; } //總資料數量

        public int TotalPages => (int)Math.Ceiling((double)TotalCount / PageSize); //總頁數
        //(轉換整數)無條件進位((轉成浮點)總筆數/每頁筆數)

        public string? SortBy { get; set; } = "UserName"; //預設是使用者名稱排序
        // 排序：記錄使用者選了哪一個項目（例如：姓名、文章數、追蹤數）

        public string? SortOrder { get; set; } = "asc"; //預設是由小到大排序
        //排序方向：記錄是「由大到小」(desc倒序)還是「由小到大」(asc正序)

        public List<SocialItemViewModel> SocialItems { get; set; } = new List<SocialItemViewModel>();
        //所有使用者的資料列表
        //預設給一個空List避免報錯
    }
}
