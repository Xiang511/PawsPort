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

        public async Task<List<PlayerListDTO>> GetPlayerListAsync()
        {
            // 由於內部包含複雜選取與子查詢，建議先將主表非同步取出，
            // 或是直接在 IQueryable 上使用 ToListAsync()
            return await _db.PlayerProfiles
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
                            // 注意：在 Select 內部使用同步 FirstOrDefault 是 EF Core 允許的轉譯，但外層必須非同步結束
                            SkinName = _db.SkinShops.FirstOrDefault(s => s.SkinId == l.SkinId).SkinName ?? "未知造型"
                        }).OrderByDescending(l => l.CreateTime).ToList(),
                    PointRecords = _db.PointTransactions.Where(pr => pr.PlayerId == p.PlayerId).OrderByDescending(pr => pr.TransactionDate).ToList()
                }).ToListAsync();
        }

        public async Task UpdatePlayerAsync(PlayerEditDTO EditDTO)
        {
            var player = await _db.PlayerProfiles.FirstOrDefaultAsync(p => p.PlayerId == EditDTO.PlayerId);
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
                var inventory = await _db.Inventories.FirstOrDefaultAsync(i =>  i.SkinId == EditDTO.SkinId && i.PlayerId == EditDTO.PlayerId);
                if (inventory == null) {
                    throw new Exception("庫存不存在");
                }


                inventory.Enable = EditDTO.Enable;
                Log.Information("PlayerService: 玩家 {PlayerId} 與庫存更新完成", EditDTO);
                //if (inventory != null)
                //{
                //    Log.Information("finish");
                //}
                

                _db.SaveChanges();
            //}
        }

        public async Task DeletePlayerAsync(int id)
        {
            var player = await _db.PlayerProfiles.FirstOrDefaultAsync(p => p.PlayerId == id);
            if (player == null)
                throw new Exception("玩家不存在");

            // 移除關聯資料
            var inventories = _db.Inventories.Where(i => i.PlayerId == id);
            var histories = _db.GameHistories.Where(h => h.PlayerId == id);

            _db.Inventories.RemoveRange(inventories);
            _db.GameHistories.RemoveRange(histories);
            _db.PlayerProfiles.Remove(player);

            await _db.SaveChangesAsync();
            Log.Information("PlayerService: 玩家 ID {id} 及其關聯資料已刪除", id);
        }

    }
}