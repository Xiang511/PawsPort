using Microsoft.AspNetCore.Mvc;
using PawsPort.Models;

namespace PawsPort.Controllers
{
    public class PlayerController : Controller
    {
        public IActionResult List()
        {
            PetDbContext db = new PetDbContext();
            var playerList = db.PlayerProfiles
                .Select(p => new
                {
                    p.PlayerId,
                    p.Point,
                    SkinCount = db.Inventories
                        .Where(i => i.PlayerId == p.PlayerId && i.Enable)
                        .Select(i => i.SkinId)
                        .Distinct()
                        .Count(),
                    IsDisabled = false, // 如果 PlayerProfile 中沒有 IsDisabled 欄位，先設為 false
                    MaxGameId = db.GameHistories
                        .Where(h => h.PlayerId == p.PlayerId)
                        .OrderByDescending(h => h.GameId)
                        .Select(h => h.GameId)
                        .FirstOrDefault(),
                    LastPlayedDate = db.GameHistories
                        .Where(h => h.PlayerId == p.PlayerId)
                        .OrderByDescending(h => h.LastPlayedDate)
                        .Select(h => h.LastPlayedDate)
                        .FirstOrDefault()
                })
                .ToList();

            return View(playerList);
        }
        // 編輯玩家 - GET 方法（用於載入 Modal 中的詳細資訊）
        public IActionResult GetPlayerDetails(int playerId)
        {
            PetDbContext db = new PetDbContext();

            var player = db.PlayerProfiles.FirstOrDefault(p => p.PlayerId == playerId);
            if (player == null)
                return NotFound();

            var skins = db.Inventories
                .Where(i => i.PlayerId == playerId)
                .Select(i => new
                {
                    i.InventoryId,
                    i.SkinId,
                    i.Enable
                })
                .ToList();

            var gameHistory = db.GameHistories
                .Where(h => h.PlayerId == playerId)
                .OrderByDescending(h => h.GameId)
                .FirstOrDefault();

            string lastPlayedDateStr = "未遊玩";
            if (gameHistory?.LastPlayedDate != null)
            {
                lastPlayedDateStr = gameHistory.LastPlayedDate.Value.ToString("yyyy-MM-dd HH:mm");
            }

            return Json(new
            {
                playerId = player.PlayerId,
                point = player.Point,
                skins = skins,
                maxGameId = gameHistory?.GameId ?? 0,
                lastPlayedDate = lastPlayedDateStr
            });
        }

        // 編輯玩家 - POST 方法
        [HttpPost]
        public IActionResult Edit(int PlayerId, int Point, int[] EnabledSkinIds)
        {
            PetDbContext db = new PetDbContext();

            // 更新玩家點數
            PlayerProfile player = db.PlayerProfiles.FirstOrDefault(p => p.PlayerId == PlayerId);
            if (player != null)
            {
                player.Point = Point;
                db.SaveChanges();
            }

            // 更新造型的啟用/禁用狀態
            var inventories = db.Inventories.Where(i => i.PlayerId == PlayerId).ToList();
            foreach (var inventory in inventories)
            {
                // 如果 EnabledSkinIds 中包含該造型的 InventoryId，則設為 true，否則設為 false
                inventory.Enable = EnabledSkinIds != null && EnabledSkinIds.Contains(inventory.InventoryId);
            }
            db.SaveChanges();

            return RedirectToAction("List");
        }

        // 刪除玩家 - POST 方法
        [HttpPost]
        public IActionResult Delete(int id)
        {
            PetDbContext db = new PetDbContext();
            PlayerProfile player = db.PlayerProfiles.FirstOrDefault(p => p.PlayerId == id);

            if (player != null)
            {
                var inventories = db.Inventories.Where(i => i.PlayerId == id).ToList();
                var histories = db.GameHistories.Where(h => h.PlayerId == id).ToList();

                db.Inventories.RemoveRange(inventories);
                db.GameHistories.RemoveRange(histories);
                db.PlayerProfiles.Remove(player);

                db.SaveChanges();
            }

            return RedirectToAction("List");
        }

    }
}
