namespace PawsPort.ViewModels
{
    public class ChatroomListViewModel
    {

        // 全站儀表板數據
        public int TotalMessages24h { get; set; }
        public int ActiveRoomsCount { get; set; }
        public int SuspiciousUserCount { get; set; } //疑似異常帳號

        // 搜尋參數
        public string TxtKeyword { get; set; }

        // 聊天室清單 (小包包)
        public List<ChatroomItemViewModel> Chatrooms { get; set; }
    }

    public class ChatroomItemViewModel
    {
        public int ChatroomId { get; set; }
        public string RoomName { get; set; }
        public int MemberCount { get; set; }
        
        // 新增：如果是 1對1，直接顯示對方是誰
        public string OtherSideName { get; set; }
        public DateTime LastMessageTime { get; set; }
        public int MsgCount24h { get; set; }

        // 雖然資料庫沒欄位，但我們可以透過邏輯判斷是否「異常熱鬧」
        public bool IsHot => MsgCount24h > 100;
    }

}
