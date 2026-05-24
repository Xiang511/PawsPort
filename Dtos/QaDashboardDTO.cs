namespace PawsPort.Dtos
{
    public class QaDashboardDTO
    {
        // 圖表 A：問題類型統計
        public Dictionary<string, int> QuestionTypeStats { get; set; }

        // 圖表 B：處理狀態統計
        public Dictionary<string, int> StatusStats { get; set; }
    }
}
