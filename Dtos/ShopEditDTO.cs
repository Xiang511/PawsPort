namespace PawsPort.Dtos
{
    public class ShopEditDTO
    {
        public int SkinId { get; set; }
        public string SkinName { get; set; }
        public string Description { get; set; }
        public int Price { get; set; }
        public bool IsAvailable { get; set; }
        public string? ImageBase64 { get; set; }
    }
}
