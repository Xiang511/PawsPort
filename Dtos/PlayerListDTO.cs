using PawsPort.Models;

namespace PawsPort.Dtos
{
    public class PlayerListDTO
    {
        public int PlayerId { get; set; }
        public int? CurrentPoint { get; set; }
        public int SkinCount { get; set; }
        public bool IsDisabled { get; set; }
        public int? MaxGameId { get; set; }
        public DateTime? LastPlayedDate { get; set; }
        public List<InventoryLogDTO> InventoryLogs { get; set; }
        public List<PointTransaction> PointRecords { get; set; }
    }

    
}