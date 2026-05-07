namespace PawsPort.Dtos
{
    
    
    // 3. 給 Edit 用的 DTO (只保留允許被修改的欄位)
    public class HealthPassportEditDto
    {
        public int PassportId { get; set; }
        public decimal? Weight { get; set; }
        public string? Note { get; set; }
        public int? RecordType { get; set; }
    }

    
    
}