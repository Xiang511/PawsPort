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
        public IActionResult List(int page = 1)
        {
            var allPlayers = _playerService.GetPlayerList();

            // 分頁邏輯保留在 Controller，因為這是呈現層細節
            int pageSize = 10;
            var pagedList = allPlayers.Skip((page - 1) * pageSize).Take(pageSize).ToList();

            return Success(new
            {
                Data = pagedList,
                CurrentPage = page,
                TotalCount = allPlayers.Count
            });
        }

        // PUT /api/Player/{id}
        [HttpPut("{id}")]
        public IActionResult Edit(PlayerEditDTO EditDTO)
        {
            Log.Information("playerid: {playerid}", EditDTO.PlayerId);
            Log.Information("point: {point}", EditDTO.Point);
            Log.Information("skinId: {skinId}", EditDTO.SkinId);
            Log.Information("Enable: {Enable}", EditDTO.Enable);
            Log.Information("-------------------------------");
            _playerService.UpdatePlayer(EditDTO);
            return Success(EditDTO, "更新成功", 200);
        }

        // DELETE /api/Player/{id}
        [HttpDelete("{id}")]
        public IActionResult Delete(int id)
        {
            _playerService.DeletePlayer(id);
            return Success(id, "刪除成功", 200);
        }
    }
}
