using Azure;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PawsPort.Dtos;
using PawsPort.Models;
using PawsPort.Services;
using Serilog;

namespace PawsPort.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Produces("application/json")]
    [Tags("遊戲系統")]
    public class PlayerController : ApiControllerBase
    {
        private readonly PlayerService _playerService;

        public PlayerController(PlayerService playerService)
        {
            _playerService = playerService;
        }

        // GET /api/Player
        /// <summary>
        /// 取得玩家列表（分頁）
        /// </summary>
        /// <param name="page">頁碼（預設為 1）</param>
        /// <returns>分頁後的玩家列表與總筆數</returns>
        /// <response code="200">成功取得玩家列表</response>
        [Authorize(Policy = "遊戲系統_普通管理員")]
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> List(int page = 1)
        {
            try
            {
                var allPlayers = await _playerService.GetPlayerListAsync();

                int pageSize = 10;
                var pagedList = allPlayers.Skip((page - 1) * pageSize).Take(pageSize).ToList();

                return Success(new
                {
                    Data = pagedList,
                    CurrentPage = page,
                    TotalCount = allPlayers.Count
                }, "取得玩家列表成功", 200);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "PlayerController: 取得列表失敗");
                return Failure("PLAYER_LIST_FAILED", "伺服器讀取玩家資料失敗", 500);
            }
        }

        // PUT /api/Player/{id}
        /// <summary>
        /// 更新玩家資訊與庫存資料
        /// </summary>
        /// <param name="id">玩家 ID</param>
        /// <param name="EditDTO">更新的玩家資料 DTO</param>
        /// <returns>更新後的玩家資料</returns>
        /// <response code="200">成功更新玩家資訊</response>
        /// <response code="400">資料格式錯誤或 ID 不一致</response>
        /// <response code="404">找不到該玩家或對應庫存</response>
        [Authorize(Policy = "遊戲系統_普通管理員")]
        [HttpPut("{id}")]
        [ProducesResponseType(typeof(PlayerEditDTO), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Edit(int id, [FromBody] PlayerEditDTO EditDTO)
        {
            if (EditDTO == null) return Failure("BAD_REQUEST", "收到的資料為空", 400);
            if (id != EditDTO.PlayerId) return Failure("PLAYER_ID_MISMATCH", "網址 ID 與資料 ID 不符", 400);

            try
            {
                await _playerService.UpdatePlayerAsync(EditDTO);
                return Success(EditDTO, "更新成功", 200);
            }
            catch (Exception ex)
            {
                if (ex.Message == "玩家不存在" || ex.Message == "庫存不存在")
                    return Failure("PLAYER_NOT_FOUND", ex.Message, 404);

                Log.Error(ex, "PlayerController: 更新玩家 {id} 失敗", id);
                return Failure("PLAYER_UPDATE_FAILED", "更新過程發生錯誤", 500);
            }
        }

        // DELETE /api/Player/{id}
        /// <summary>
        /// 刪除指定玩家
        /// </summary>
        /// <param name="id">玩家 ID</param>
        /// <response code="200">成功刪除玩家</response>
        /// <response code="404">找不到該玩家</response>
        [Authorize(Policy = "遊戲系統_普通管理員")]
        [HttpDelete("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Delete(int id)
        {
            Log.Information("正在準備刪除玩家 ID: {id}", id);
            try
            {
                await _playerService.DeletePlayerAsync(id);
                return Success(id, "刪除成功", 200);
            }
            catch (Exception ex)
            {
                if (ex.Message == "玩家不存在")
                    return Failure("PLAYER_NOT_FOUND", "找不到玩家", 404);

                Log.Error(ex, "PlayerController: 刪除玩家 {id} 失敗", id);
                return Failure("PLAYER_DELETE_FAILED", "刪除過程發生錯誤", 500);
            }
        }

        // GET /api/Player/search?query=xxx&page=1
        /// <summary>
        /// 搜尋玩家
        /// </summary>
        /// <param name="query">搜尋關鍵字（名稱或相關資訊）</param>
        /// <param name="page">頁碼（預設為 1）</param>
        /// <returns>搜尋結果清單</returns>
        /// <response code="200">成功完成搜尋</response>
        [Authorize(Policy = "遊戲系統_普通管理員")]
        [HttpGet("search")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> Search([FromQuery] string query, int page = 1)
        {
            try
            {
                // 1. 呼叫 Service 取得搜尋後的結果，此時 searchResults 已經是 List<PlayerListDTO>
                var searchResults = await _playerService.SearchPlayersAsync(query);

                int pageSize = 10;
                var pagedList = searchResults
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .ToList();

                return Success(new
                {
                    Data = pagedList,      
                    CurrentPage = page,
                    TotalCount = searchResults.Count
                }, $"搜尋「{query}」成功", 200);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "PlayerController: 搜尋玩家失敗");
                return Failure("PLAYER_SEARCH_FAILED", "搜尋過程發生錯誤", 500);
            }
        }

        // GET /api/Player/{id}/logs
        /// <summary>
        /// 取得玩家相關紀錄（如異動日誌）
        /// </summary>
        /// <param name="id">玩家 ID</param>
        /// <returns>玩家的異動紀錄列表</returns>
        /// <response code="200">成功取得紀錄</response>
        [Authorize(Policy = "遊戲系統_普通管理員")]
        [HttpGet("{id}/logs")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> GetPlayerLogs(int id)
        {
            try
            {
                var records = await _playerService.GetPlayerRecordsAsync(id);
                return Success(records, "取得玩家紀錄成功", 200);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "取得紀錄失敗");
                return Failure("GET_LOGS_FAILED", "伺服器錯誤", 500);
            }
        }


        // GET /api/Player/{id}/game-history
        /// <summary>
        /// 遊戲前台：撈取玩家所有關卡的通關歷史紀錄
        /// </summary>
        [Authorize(Policy = "遊戲系統_一般成員")]
        [HttpGet("{id}/game-history")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> GetGameHistory(int id)
        {
            try
            {
                var history = await _playerService.GetPlayerGameHistoryAsync(id);
                return Success(history, "成功取得遊戲歷史紀錄", 200); // 採用你們的統一回傳格式
            }
            catch (Exception ex)
            {
                Log.Error(ex, "PlayerController: 取得遊戲歷史紀錄失敗");
                return Failure("GAME_HISTORY_GET_FAILED", "取得紀錄發生錯誤", 500);
            }
        }

        // POST /api/Player/save-game-result
        /// <summary>
        /// 遊戲前台：遊戲結算，儲存歷史進度並發放獎勵點數
        /// </summary>
        [Authorize(Policy = "遊戲系統_一般成員")]
        [HttpPost("save-game-result")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> SaveGameResult([FromBody] GameResultSubmitDTO dto)
        {
            try
            {
                if (dto == null) return Failure("INVALID_DATA", "傳入資料不能為空", 400);

                bool result = await _playerService.SaveGameProgressAsync(
                    dto.PlayerId, dto.GameId, dto.IsVictory, dto.BonusPoints
                );

                return Success(new { Success = true }, "小遊戲結算成功，已同步至資料庫！", 200);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "PlayerController: 儲存小遊戲結算失敗");
                return Failure("GAME_RESULT_SAVE_FAILED", "儲存結算資料發生錯誤", 500);
            }
        }

        // PUT /api/Player/{playerId}/equip-skin
        /// <summary>
        /// 遊戲前台：玩家編輯裝備造型
        /// </summary>
        [Authorize(Policy = "遊戲系統_一般成員")]
        [HttpPut("{playerId}/equip-skin")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> EquipSkin(int playerId, [FromBody] EquipSkinDTO dto)
        {
            if (dto == null || dto.SkinId <= 0)
                return Failure("INVALID_DATA", "造型 ID 不能為空", 400);

            try
            {
                await _playerService.EquipSkinAsync(playerId, dto.SkinId);
                return Success(new { Success = true }, "造型裝備成功", 200);
            }
            catch (Exception ex)
            {
                if (ex.Message.Contains("不存在") || ex.Message.Contains("未擁有"))
                    return Failure("EQUIP_SKIN_FAILED", ex.Message, 404);

                Log.Error(ex, "PlayerController: 裝備造型失敗");
                return Failure("EQUIP_SKIN_FAILED", "裝備造型失敗", 500);
            }
        }

        // POST /api/Player/{playerId}/buy-skin
        /// <summary>
        /// 遊戲前台：玩家購買造型
        /// </summary>
        [Authorize(Policy = "遊戲系統_一般成員")]
        [HttpPost("{playerId}/buy-skin")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> BuySkin(int playerId, [FromBody] BuySkinDTO dto)
        {
            if (dto == null || dto.SkinId <= 0)
                return Failure("INVALID_DATA", "造型 ID 不能為空", 400);

            try
            {
                var (remainingPoints, acquiredSkinId) = await _playerService.BuySkinAsync(playerId, dto.SkinId);
                return Success(new { remainingPoints, acquiredSkinId }, "購買成功，已添加到收藏", 200);
            }
            catch (Exception ex)
            {
                if (ex.Message.Contains("不存在") || ex.Message.Contains("已擁有"))
                    return Failure("BUY_SKIN_FAILED", ex.Message, 404);
                if (ex.Message.Contains("點數不足"))
                    return Failure("INSUFFICIENT_POINTS", "點數不足", 400);

                Log.Error(ex, "PlayerController: 購買造型失敗");
                return Failure("BUY_SKIN_FAILED", "購買失敗", 500);
            }
        }


        // GET /api/Player/{playerId}/inventory
        /// <summary>
        /// 遊戲前台：取得玩家收藏庫內容
        /// </summary>
        [Authorize(Policy = "遊戲系統_一般成員")]
        [HttpGet("{playerId}/inventory")]
        [ProducesResponseType(typeof(List<PlayerSkinDTO>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetInventory(int playerId)
        {
            try
            {
                var inventory = await _playerService.GetInventoryAsync(playerId);
                return Success(inventory, "獲得玩家收藏庫成功", 200);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "PlayerController: 獲得玩家收藏庫失敗");
                return Failure("GET_INVENTORY_FAILED", "獲得玩家收藏庫失敗", 500);
            }
        }

        // GET /api/Player/{playerId}
        /// <summary>
        /// 遊戲前台：根據 PlayerId 取得玩家資料
        /// </summary>
        [Authorize(Policy = "遊戲系統_一般成員")]
        [HttpGet("{playerId}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> GetPlayerById(int playerId)
        {
            try
            {
                var player = await _playerService.GetPlayerByIdAsync(playerId);
                if (player == null)
                    return Failure("PLAYER_NOT_FOUND", "玩家不存在", 404);

                return Success(player, "成功取得玩家資料", 200);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "PlayerController: 取得玩家資料失敗");
                return Failure("GET_PLAYER_FAILED", "取得玩家資料失敗", 500);
            }
        }


    }

}
    
