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
            return await _db.PlayerProfiles
                .Select(p => new PlayerListDTO
                {
                    PlayerId = p.PlayerId,
                    UserName = p.UserName,
                    CurrentPoint = p.CurrentPoint ?? 0,

                    // 取得該玩家擁有的所有造型詳細資訊
                    OwnedSkins = _db.Inventories
                        .Where(i => i.PlayerId == p.PlayerId)
                        .Join(_db.SkinShops, // 這裡主動與 SkinShop 表做 Join
                            i => i.SkinId,   // Inventory 的外鍵
                            s => s.SkinId,   // SkinShop 的主鍵
                            (i, s) => new PlayerSkinDTO // 組合成 DTO
                        {
                            SkinId = s.SkinId,
                            SkinName = s.SkinName,
                            SkinImage = s.SkinImage,
                            Enable = i.Enable
                        }).ToList(),

                    // 玩家帳號建立時間：取 Inventory 中最早的那一筆 CreateTime
                    CreateTime = _db.Inventories
                        .Where(i => i.PlayerId == p.PlayerId)
                        .OrderBy(i => i.CreateTime)
                        .Select(i => i.CreateTime)
                        .FirstOrDefault(),

                    // 持有造型數量：計算 Inventory 中該玩家所有的造型 (包含未 Enable 的)
                    SkinCount = _db.Inventories
                        .Where(i => i.PlayerId == p.PlayerId)
                        .Count(),

                    // 目前啟用的造型 ID (供前端顯示頭像使用)
                    EnabledSkinId = _db.Inventories
                        .Where(i => i.PlayerId == p.PlayerId && i.Enable)
                        .Select(i => (int?)i.SkinId)
                        .FirstOrDefault(),

                    // 遊玩進度：取 GameHistory 中最大的 GameId
                    MaxGameId = _db.GameHistories
                        .Where(h => h.PlayerId == p.PlayerId)
                        .Max(h => (int?)h.GameId) ?? 0,

                    // 最後遊玩時間
                    LastPlayedDate = _db.GameHistories
                        .Where(h => h.PlayerId == p.PlayerId)
                        .OrderByDescending(h => h.LastPlayedDate)
                        .Select(h => h.LastPlayedDate)
                        .FirstOrDefault()
                })
                .ToListAsync();
        }

        public async Task UpdatePlayerAsync(PlayerEditDTO EditDTO)
        {
            // 1. 查找玩家主表
            var player = await _db.PlayerProfiles.FirstOrDefaultAsync(p => p.PlayerId == EditDTO.PlayerId);
            if (player == null) throw new Exception("玩家不存在");

            // 更新後台可修改的數值 (例如點數)
            player.CurrentPoint = EditDTO.Point;

            // 2. 查找該玩家的特定造型庫存
            var targetInventory = await _db.Inventories
                .FirstOrDefaultAsync(i => i.SkinId == EditDTO.SkinId && i.PlayerId == EditDTO.PlayerId);

            if (targetInventory != null)
            {
                // 關鍵：如果後台點選了「啟用」這個造型 (Enable 為 true)
                // 「頭像只能有一個」
                if (EditDTO.Enable)
                {
                    // 先找出該玩家目前「其他」所有被啟用的造型，將它們設為 false
                    var otherEnabledSkins = _db.Inventories
                        .Where(i => i.PlayerId == EditDTO.PlayerId && i.SkinId != EditDTO.SkinId && i.Enable);

                    foreach (var s in otherEnabledSkins)
                    {
                        s.Enable = false;
                    }
                }

                // 最後設定目標造型的狀態 (無論是改為 true 還是 false)
                targetInventory.Enable = EditDTO.Enable;
            }

            await _db.SaveChangesAsync();
            Log.Information("PlayerService: 玩家 {PlayerId} 的資料與造型 {SkinId} 狀態已更新", EditDTO.PlayerId, EditDTO.SkinId);
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