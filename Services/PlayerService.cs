using Microsoft.EntityFrameworkCore;
using PawsPort.Dtos;
using PawsPort.Models;
using Serilog;

namespace PawsPort.Services
{
    public class PlayerService
    {
        private readonly PetDbContext _db;

        public PlayerService(PetDbContext db)
        {
            _db = db;
        }

        public List<PlayerListDTO> GetPlayerList()
        {
            return _db.PlayerProfiles
                .Select(p => new PlayerListDTO
                {
                    PlayerId = p.PlayerId,
                    CurrentPoint = p.CurrentPoint,
                    SkinCount = _db.Inventories
                        .Where(i => i.PlayerId == p.PlayerId && i.Enable)
                        .Join(_db.SkinShops.Where(s => s.IsAvailable && s.IsDel != true),
                              i => i.SkinId, s => s.SkinId, (i, s) => i)
                        .Select(i => i.SkinId).Distinct().Count(),
                    IsDisabled = false,
                    MaxGameId = _db.GameHistories.Where(h => h.PlayerId == p.PlayerId).OrderByDescending(h => h.GameId).Select(h => h.GameId).FirstOrDefault(),
                    LastPlayedDate = _db.GameHistories.Where(h => h.PlayerId == p.PlayerId).OrderByDescending(h => h.LastPlayedDate).Select(h => h.LastPlayedDate).FirstOrDefault(),
                    InventoryLogs = _db.ItemAcquisitionLogs.Where(l => l.PlayerId == p.PlayerId)
                        .Select(l => new InventoryLogDTO
                        {
                            LogId = l.LogId,
                            PlayerId = l.PlayerId,
                            SkinId = l.SkinId,
                            CreateTime = l.CreateTime,
                            AcquireType = l.AcquireType,
                            SkinName = _db.SkinShops.FirstOrDefault(s => s.SkinId == l.SkinId).SkinName ?? "未知造型"
                        }).OrderByDescending(l => l.CreateTime).ToList(),
                    PointRecords = _db.PointTransactions.Where(pr => pr.PlayerId == p.PlayerId).OrderByDescending(pr => pr.TransactionDate).ToList()
                }).ToList();
        }

        public void UpdatePlayer(PlayerEditDTO EditDTO)
        {
            var player = _db.PlayerProfiles.FirstOrDefault(p => p.PlayerId == EditDTO.PlayerId);
            if (player == null)
                throw new Exception("玩家不存在");

            // 玩家存在
            Log.Information("finish player");
            //if (player != null)
            //{
                Log.Information("找到使用者");
                player.CurrentPoint = EditDTO.Point;
                Log.Information("點數已變更");

                //var inventories = _db.Inventories.Where(i => i.PlayerId == playerId).ToList();
                //foreach (var inv in inventories)
                //{
                //    inv.Enable = enabledSkinIds != null && enabledSkinIds.Contains(inv.InventoryId);
                //}
                Log.Information("SkinId: {SkinId}", EditDTO.SkinId);
                Log.Information("playerId: {playerId}", EditDTO.PlayerId);
                var inventory = _db.Inventories.FirstOrDefault(i =>  i.SkinId == EditDTO.SkinId && i.PlayerId == EditDTO.PlayerId);
                if (inventory == null) {
                    throw new Exception("庫存不存在");
                }


                inventory.Enable = EditDTO.Enable;
                Log.Information("已經找到目標{inventory}", inventory);
                //if (inventory != null)
                //{
                //    Log.Information("finish");
                //}
                

                _db.SaveChanges();
            //}
        }

        public void DeletePlayer(int id)
        {
            var player = _db.PlayerProfiles.FirstOrDefault(p => p.PlayerId == id);
            if (player == null)
                throw new Exception("玩家不存在");
            //if (player != null)
            //{
                _db.Inventories.RemoveRange(_db.Inventories.Where(i => i.PlayerId == id));
                _db.GameHistories.RemoveRange(_db.GameHistories.Where(h => h.PlayerId == id));
                _db.PlayerProfiles.Remove(player);
                _db.SaveChanges();
            //}
        }

    }
}