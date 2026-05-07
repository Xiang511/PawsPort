namespace PawsPort.Dtos
{
    public class InventoryLogDTO
    {
        public int LogId { get; set; }
        public int PlayerId { get; set; }
        public int? SkinId { get; set; }
        public DateTime? CreateTime { get; set; }
        public string AcquireType { get; set; }
        public string SkinName { get; set; }
    }
}
