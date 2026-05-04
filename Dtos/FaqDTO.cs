namespace PawsPort.Dtos
{
    public class FaqDTO
    {
        public int Faqid { get; set; }
        public string Question { get; set; }
        public string Answer { get; set; }
        // 只需要放前端會用到的欄位，不要放 IsExist 或 CreateDate 等機密資訊
    }
}
