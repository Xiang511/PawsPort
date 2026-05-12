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
    [Tags("權限管理 / 權限資訊")]
    public class PermissionsController : ApiControllerBase
    {
        private readonly MemberPermissionService _memberPermissionService;
        private readonly MemberBlockListService _memberBlockListService;

        public PermissionsController(PetDbContext context, MemberPermissionService memberPermissionService, MemberBlockListService memberBlockListService)
        {
            _memberPermissionService = memberPermissionService;
            _memberBlockListService = memberBlockListService;
        }

        /// <summary>
        /// 取得所有使用者的權限列表
        /// </summary>
        /// <returns>使用者權限列表 JSON</returns>
        /// <response code="200">成功取得使用者權限列表</response>

        [HttpGet("users")]
        [ProducesResponseType(typeof(IEnumerable<MemberPermissionUserDTO>), StatusCodes.Status200OK)]
        [Tags("權限管理 / 權限資訊")]
        public async Task<IActionResult> UserPermission()
        {

            var userPermissions = await _memberPermissionService.GetAllUserPermissionAsync();
            Log.Debug("取得使用者權限列表成功，總數: {Count}", userPermissions.Count());
            return Success(userPermissions, "Success", 200);
        }
        /// <summary>
        /// 取得所有系統列表
        /// </summary>
        /// <returns>系統列表 JSON</returns>
        /// <response code="200">成功取得系統列表</response>
        [HttpGet("systems")]
        [ProducesResponseType(typeof(MemberPermissionSystemDTO), StatusCodes.Status200OK)]
        [Tags("權限管理 / 權限資訊")]

        public async Task<IActionResult> System()
        {

            var result = await _memberPermissionService.GetMemberPermissionSystemAsync();

            Log.Debug("取得系統列表成功，總數: {Count}", result.Systems.Count());
            return Success(result, "Success", 200);
        }

        /// <summary>
        /// 取得所有角色列表
        /// </summary>
        /// <returns>角色列表 JSON</returns>
        /// <response code="200">成功取得角色列表</response>

        [HttpGet("roles")]
        [ProducesResponseType(typeof(MemberPermissionRoleDTO), StatusCodes.Status200OK)]
        [Tags("權限管理 / 權限資訊")]

        public async Task<IActionResult> Roles()
        {

            var result = await _memberPermissionService.GetMemberPermissionRoleAsync();


            Log.Debug("取得角色列表成功，總數: {Count}", result.Roles.Count());
            return Success(result, "Success", 200);
        }
        /// <summary>
        /// 取得所有被封鎖的會員列表
        /// </summary>
        /// <returns>被封鎖的會員列表</returns>
        /// <response code="200">成功取得被封鎖的會員列表</response>
        [HttpGet("block/users")]
        [ProducesResponseType(typeof(List<MemberBlockListDTO>), StatusCodes.Status200OK)]
        [Tags("權限管理 / 違規資訊")]
        public async Task<IActionResult> BlockList()
        {
            Log.Debug("[BlacklistController] BlockList GET - Entry");

            Log.Debug("[BlacklistController] 調用 MemberBlockListService.GetBannedUsersAsync");
            var bannedUsers = await _memberBlockListService.GetBannedUsersAsync();

            Log.Debug("[BlacklistController] 成功取得被封鎖的會員列表, 共 {Count} 筆", bannedUsers.Count);
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
        [HttpPatch("block/users/{id}")]
        [ProducesResponseType(typeof(MemberBlockListDTO), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [Tags("權限管理 / 違規資訊")]
        public async Task<IActionResult> BlockListCreate(int? id, MemberBlockListEditDTO user)
        {
            Log.Debug("[BlacklistController] BlockListCreate POST - Entry, UserId: {UserId}", id);

            Log.Debug("[BlacklistController] 調用 MemberBlockListService.CreateBannedUsersAsync, UserId: {UserId}", id);
            var result = await _memberBlockListService.CreateBannedUsersAsync(id, user);

            Log.Debug("[BlacklistController] 封鎖會員成功, UserId: {UserId}, 名稱: {Name}", id, result.Name);
            return Success(result, "Success", 200);
        }
    }
}
