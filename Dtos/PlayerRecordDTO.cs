namespace PawsPort.Dtos
{
    public class PlayerRecordDTO
    {
        // 消費紀錄 (Amount < 0)
        public List<PointChangeDTO> ConsumptionLogs { get; set; } = new();
        // 點數來源 (Amount > 0)
        public List<PointChangeDTO> PointLogs { get; set; } = new();
    }

    public class PointChangeDTO
    {
        public DateTime TransactionDate { get; set; }
        public string Description { get; set; } // 顯示造型名稱或遊戲關卡
        public int Amount { get; set; }
    }
}
