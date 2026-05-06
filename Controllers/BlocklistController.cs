using Microsoft.AspNetCore.Mvc;
using PawsPort.Dtos;
using PawsPort.Models;
using PawsPort.Services;
using PawsPort.ViewModels;
using Serilog;
namespace PawsPort.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Produces("application/json")]
    [Tags("封鎖管理")]

    public class BlocklistController : ApiControllerBase
    {
        private readonly MemberBlockListService _memberBlockListService;

        public BlocklistController(PetDbContext context, MemberBlockListService memberBlockListService)
        {
            _memberBlockListService = memberBlockListService;
        }
        /// <summary>
        /// 取得所有被封鎖的會員列表
        /// </summary>
        /// <returns>被封鎖的會員列表</returns>
        /// <response code="200">成功取得被封鎖的會員列表</response>
        [HttpGet("users")]
        [ProducesResponseType(typeof(List<MemberBlockListDTO>), StatusCodes.Status200OK)]
        public async Task<IActionResult> BlockList()
        {
            Log.Information("[BlacklistController] BlockList GET - Entry");

            Log.Information("[BlacklistController] 調用 MemberBlockListService.GetBannedUsersAsync");
            var bannedUsers = await _memberBlockListService.GetBannedUsersAsync();

            Log.Information("[BlacklistController] 成功取得被封鎖的會員列表, 共 {Count} 筆", bannedUsers.Count);
            return Success(bannedUsers, "Success", 200);

        }

        /// <summary>
        /// 封鎖/解封指定會員
        /// </summary>
        /// <param name="id">會員 ID</param>
        /// <param name="user">封鎖資料（包含封鎖原因等資訊）</param>
        /// <returns>封鎖成功的會員資料</returns>
        /// <response code="200">成功封鎖會員</response>
        /// <response code="400">請求資料格式錯誤</response>
        /// <response code="404">找不到指定的會員</response>
        [HttpPatch("users/{id}")]
        [ProducesResponseType(typeof(MemberBlockListDTO), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> BlockListCreate(int? id , MemberBlockListEditDTO user)
        {
            Log.Information("[BlacklistController] BlockListCreate POST - Entry, UserId: {UserId}", id);

            Log.Information("[BlacklistController] 調用 MemberBlockListService.CreateBannedUsersAsync, UserId: {UserId}", id);
            var result = await _memberBlockListService.CreateBannedUsersAsync(id,user);

            Log.Information("[BlacklistController] 封鎖會員成功, UserId: {UserId}, 名稱: {Name}", id, result.Name);
            return Success(result, "Success", 200);
        }
    }
}
