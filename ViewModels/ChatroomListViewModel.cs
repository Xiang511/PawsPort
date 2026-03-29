namespace PawsPort.ViewModels
{
    public class ChatroomListViewModel
    {

        // 全站儀表板數據
        public int TotalMsg24h { get; set; }
        public int ActiveRoomsCount { get; set; }
        public int SuspiciousUserCount { get; set; } //疑似異常帳號

        // 搜尋參數
        public string TxtKeyword { get; set; }

        //分頁
        private int _currentPage = 1;
        public int CurrentPage
        {//目前頁數
            get => _currentPage;
            set => _currentPage = value < 1 ? 1 : value;
        }
        // 只要設進來的值小於 1，通通變回 1} = 1;

        public int PageSize { get; set; } = 10; //預設10筆
        //每頁顯示的資料數量
        public int TotalCount { get; set; } //總資料數量

        public int TotalPages => (int)Math.Ceiling((double)TotalCount / PageSize); //總頁數
        //(轉換整數)無條件進位((轉成浮點)總筆數/每頁筆數)

        public string? SortBy { get; set; } = "UserName"; //預設是使用者名稱排序
        // 排序：記錄使用者選了哪一個項目（例如：姓名、文章數、追蹤數）

        public string? SortOrder { get; set; } = "asc"; //預設是由小到大排序
        //排序方向：記錄是「由大到小」(desc倒序)還是「由小到大」(asc正序)


        // 聊天室清單 (小包包)
        public List<ChatroomItemViewModel> ChatroomItems { get; set; }
    }

    public class ChatroomItemViewModel
    {
        public int ChatroomId { get; set; }

        public string UserId_1_Name { get; set; }

        public string UserId_2_Name { get; set; }

        public string ChatroomName => $"{UserId_1_Name} ⇄ {UserId_2_Name}";
        // 唯讀屬性：只要上面兩個名字有值，這個就會自動產生

        public int MemberCount { get; set; } = 2;

        public string LastMsg { get; set; }
        public DateTime? LastMsgTime { get; set; }
        public int MsgCount24h { get; set; }

        // 雖然資料庫沒欄位，但我們可以透過邏輯判斷是否「異常熱鬧」
        public bool IsHot => MsgCount24h > 100;
    }

}
