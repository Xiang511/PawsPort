using PawsPort.Models;

namespace PawsPort.Dtos
{
    public class PlayerListDTO
    {
        public int PlayerId { get; set; }
        public string UserName { get; set; }
        public DateTime? CreateTime { get; set; } // 新增此欄位
        public int CurrentPoint { get; set; }
        public int SkinCount { get; set; }
        public int? EnabledSkinId { get; set; }
        public int MaxGameId { get; set; }
        public DateTime? LastPlayedDate { get; set; }
        public List<PlayerSkinDTO> OwnedSkins { get; set; } = new();

    }
    public class PlayerSkinDTO
    {
        public int SkinId { get; set; }
        public string? SkinName { get; set; }
        public string? SkinImage { get; set; }
        public bool Enable { get; set; }
    }
}