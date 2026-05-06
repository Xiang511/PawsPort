using Azure;
using Microsoft.AspNetCore.Mvc;
using PawsPort.Dtos;
using PawsPort.Models;
using PawsPort.Services;
using Serilog;

namespace PawsPort.Controllers
{
    [Route("api/[controller]")]
    public class PlayerController : ApiControllerBase
    {
        private readonly PlayerService _playerService;

        public PlayerController(PlayerService playerService)
        {
            _playerService = playerService;
        }

        // GET /api/Player
        [HttpGet]
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
        [HttpPut("{id}")]
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
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
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
    }
}
