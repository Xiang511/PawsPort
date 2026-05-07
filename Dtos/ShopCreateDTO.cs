namespace PawsPort.Dtos
{
    public class ShopCreateDTO
    {
        public string SkinName { get; set; }
        public string Description { get; set; }
        public int Price { get; set; }
        public bool IsAvailable { get; set; }
        public string? ImageBase64 { get; set; }
    }
}
