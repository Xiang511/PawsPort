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

        public async Task<List<PlayerListDTO>> SearchPlayersAsync(string searchTerm)
        {
            // 1. 先建立基礎查詢 (Queryable)
            var query = _db.PlayerProfiles.AsNoTracking().AsQueryable();

            // 2. 套用搜尋過濾條件
            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                bool isId = int.TryParse(searchTerm, out int id);
                query = query.Where(p =>
                    (isId && p.PlayerId == id) ||
                    p.UserName.Contains(searchTerm));
            }

            // 3. 關鍵：直接套用與 GetPlayerListAsync 一模一樣的 Select 邏輯
            // 這樣能保證搜尋出來的人，其「造型數量」、「最後遊玩時間」都是正確計算過的
            return await query
                .OrderByDescending(p => p.PlayerId)
                .Select(p => new PlayerListDTO
                {
                    PlayerId = p.PlayerId,
                    UserName = p.UserName,
                    CurrentPoint = p.CurrentPoint ?? 0,

                    OwnedSkins = _db.Inventories
                        .Where(i => i.PlayerId == p.PlayerId)
                        .Join(_db.SkinShops,
                            i => i.SkinId,
                            s => s.SkinId,
                            (i, s) => new PlayerSkinDTO
                            {
                                SkinId = s.SkinId,
                                SkinName = s.SkinName,
                                SkinImage = s.SkinImage,
                                Enable = i.Enable
                            }).ToList(),

                    CreateTime = _db.Inventories
                        .Where(i => i.PlayerId == p.PlayerId)
                        .OrderBy(i => i.CreateTime)
                        .Select(i => i.CreateTime)
                        .FirstOrDefault(),

                    SkinCount = _db.Inventories
                        .Where(i => i.PlayerId == p.PlayerId)
                        .Count(),

                    EnabledSkinId = _db.Inventories
                        .Where(i => i.PlayerId == p.PlayerId && i.Enable)
                        .Select(i => (int?)i.SkinId)
                        .FirstOrDefault(),

                    MaxGameId = _db.GameHistories
                        .Where(h => h.PlayerId == p.PlayerId)
                        .Max(h => (int?)h.GameId) ?? 0,

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

            // 更新玩家名稱
            if (!string.IsNullOrWhiteSpace(EditDTO.UserName))
            {
                // 驗證名字長度
                if (EditDTO.UserName.Length > 50)
                    throw new Exception("玩家名字不能超過 50 個字");

                player.UserName = EditDTO.UserName.Trim();
            }

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
            Log.Information("PlayerService: 玩家 {PlayerId} 的資料與造型 {SkinId} 狀態已更新，名字: {UserName}", EditDTO.PlayerId, EditDTO.SkinId, EditDTO.UserName);
        }

        public async Task DeletePlayerAsync(int id)
        {
            var player = await _db.PlayerProfiles.FindAsync(id);
            if (player == null) throw new Exception("玩家不存在");

            using var transaction = await _db.Database.BeginTransactionAsync();
            try
            {
                // 找出所有關聯資料 (加上 ToListAsync 確保資料立刻被載入記憶體)
                var logs = await _db.ItemAcquisitionLogs.Where(l => l.PlayerId == id).ToListAsync();
                var trans = await _db.PointTransactions.Where(t => t.PlayerId == id).ToListAsync();
                var histories = await _db.GameHistories.Where(h => h.PlayerId == id).ToListAsync();
                var inventories = await _db.Inventories.Where(i => i.PlayerId == id).ToListAsync();

                // 依照「由子到父」的順序標記刪除
                if (logs.Any()) _db.ItemAcquisitionLogs.RemoveRange(logs);
                if (trans.Any()) _db.PointTransactions.RemoveRange(trans);
                if (histories.Any()) _db.GameHistories.RemoveRange(histories);
                if (inventories.Any()) _db.Inventories.RemoveRange(inventories);

                // 先存檔一次，清空子表
                await _db.SaveChangesAsync();

                // 最後才標記並刪除主表
                _db.PlayerProfiles.Remove(player);
                await _db.SaveChangesAsync();

                // 提交
                await transaction.CommitAsync();

                Log.Information("玩家 {id} 及其關聯資料已成功刪除", id);
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                var innerMsg = ex.InnerException?.Message ?? ex.Message;
                Log.Error("刪除失敗，錯誤詳細資訊：{Message}", innerMsg);
                throw new Exception(innerMsg);
            }
        }

        public async Task<PlayerRecordDTO> GetPlayerRecordsAsync(int playerId)
        {
            var result = new PlayerRecordDTO();

            // 1. 取得消費紀錄 (Amount < 0 且有 SkinId)
            result.ConsumptionLogs = await _db.PointTransactions
                .Where(t => t.PlayerId == playerId && t.Amount < 0)
                .Join(_db.SkinShops,
                    t => t.SkinId,
                    s => s.SkinId,
                    (t, s) => new PointChangeDTO
                    {
                        TransactionDate = t.TransactionDate,
                        Description = s.SkinName, // 取得造型名稱
                        Amount = t.Amount ?? 0        // 這會是負數，例如 -750
                    })
                .OrderByDescending(x => x.TransactionDate)
                .ToListAsync();

            // 2. 取得獲取紀錄 (Amount > 0)
            result.PointLogs = await _db.PointTransactions
                .Where(t => t.PlayerId == playerId && t.Amount > 0)
                .Select(t => new PointChangeDTO
                {
                    TransactionDate = t.TransactionDate,
                    // 如果有 GameId 則顯示關卡，否則顯示交易類型 (如: 任務獎勵)
                    Description = t.GameId > 0 ? $"遊戲第 {t.GameId} 關" : t.TransactionType,
                    Amount = t.Amount ?? 0        // 這會是正數，例如 +500
                })
                .OrderByDescending(x => x.TransactionDate)
                .ToListAsync();

            return result;
        }


        // 取得該玩家 (PlayerId) 在所有關卡的通關紀錄
        public async Task<List<GameHistory>> GetPlayerGameHistoryAsync(int playerId)
        {
            return await _db.GameHistories
                .Where(h => h.PlayerId == playerId)
                .ToListAsync();
        }

        // 儲存關卡結算、解鎖下一關、發放代幣並寫入交易日誌
        public async Task<bool> SaveGameProgressAsync(int playerId, int gameId, bool isVictory, int bonusPoints)
        {
            // 1. 檢查此玩家是否曾經玩過這一關的紀錄
            var currentHistory = await _db.GameHistories
                .FirstOrDefaultAsync(h => h.PlayerId == playerId && h.GameId == gameId);

            if (currentHistory == null)
            {
                // 第一次通關：新增紀錄
                currentHistory = new GameHistory
                {
                    PlayerId = playerId,
                    GameId = gameId,
                    StageClear = isVictory,
                    LastPlayedDate = DateTime.Now,
                    ReceivedReward = bonusPoints > 0
                };
                _db.GameHistories.Add(currentHistory);
            }
            else
            {
                // 重刷關卡：更新最後遊玩時間，且如果之前沒過而這次過了，就改為 true
                if (isVictory)
                {
                    currentHistory.StageClear = true;
                }
                currentHistory.LastPlayedDate = DateTime.Now;
                if (bonusPoints > 0)
                {
                    currentHistory.ReceivedReward = true;
                }
            }

            // 自動連鎖解鎖「下一關」
            // 根據大廳邏輯，如果第 1 關過了，就必須在 GameHistory 裡面建立一筆第 2 關的初始化資料
            if (isVictory)
            {
                int nextGameId = gameId + 1;
                // 假設總關卡到 10 關為止
                if (nextGameId <= 10)
                {
                    var nextHistory = await _db.GameHistories
                        .FirstOrDefaultAsync(h => h.PlayerId == playerId && h.GameId == nextGameId);

                    if (nextHistory == null)
                    {
                        _db.GameHistories.Add(new GameHistory
                        {
                            PlayerId = playerId,
                            GameId = nextGameId,
                            StageClear = false, // 尚未通關，但建立紀錄代表大廳可以「解鎖」它
                            LastPlayedDate = DateTime.Now,
                            ReceivedReward = false
                        });
                    }
                }
            }

            // 處理金幣獎勵（發放點數並更新主表，同時記錄到 Transaction 歷史表）
            if (bonusPoints > 0)
            {
                // 更新玩家主表 (PlayerProfile) 的 CurrentPoint
                var player = await _db.PlayerProfiles.FindAsync(playerId);
                if (player != null)
                {
                    player.CurrentPoint = (player.CurrentPoint ?? 0) + bonusPoints;
                }

                // 寫入點數交易紀錄表 (PointTransaction)
                var transaction = new PointTransaction
                {
                    PlayerId = playerId,
                    GameId = gameId,                       // 對齊你們的 GameId
                    Amount = bonusPoints,                  // 例如全對得 10 點
                    TransactionType = "遊戲關卡獎勵",
                    TransactionDate = DateTime.Now,
                    SkinId = 1,                            // 遊戲獎勵非購買造型，給予1
                    PassportId = null
                };
                _db.PointTransactions.Add(transaction);
            }

            // 執行資料庫異動儲存
            await _db.SaveChangesAsync();
            return true;
        }
    }

    
}