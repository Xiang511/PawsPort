using Azure;
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
    [Tags("遊戲系統 / 玩家管理")]
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
    }
}
