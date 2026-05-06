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
    [Tags("權限管理")]
    public class PermissionsController : ApiControllerBase
    {
        private readonly MemberPermissionService _memberPermissionService;


        public PermissionsController(PetDbContext context, MemberPermissionService memberPermissionService)
        {
            _memberPermissionService = memberPermissionService;
        }

        /// <summary>
        /// 取得所有使用者的權限列表
        /// </summary>
        /// <returns>使用者權限列表 JSON</returns>
        /// <response code="200">成功取得使用者權限列表</response>

        [HttpGet("users")]
        [ProducesResponseType(typeof(IEnumerable<MemberPermissionUserDTO>), StatusCodes.Status200OK)]

        public async Task<IActionResult> UserPermission()
        {

            var userPermissions = await _memberPermissionService.GetAllUserPermissionAsync();
            Log.Information("取得使用者權限列表成功，總數: {Count}", userPermissions.Count());
            return Success(userPermissions, "Success", 200);
        }
        /// <summary>
        /// 取得所有系統列表
        /// </summary>
        /// <returns>系統列表 JSON</returns>
        /// <response code="200">成功取得系統列表</response>
        [HttpGet("systems")]
        [ProducesResponseType(typeof(MemberPermissionSystemDTO), StatusCodes.Status200OK)]
 
        public async Task<IActionResult> System()
        {

            var result = await _memberPermissionService.GetMemberPermissionSystemAsync();

            Log.Information("取得系統列表成功，總數: {Count}", result.Systems.Count());
            return Success(result, "Success", 200);
        }

        /// <summary>
        /// 取得所有角色列表
        /// </summary>
        /// <returns>角色列表 JSON</returns>
        /// <response code="200">成功取得角色列表</response>

        [HttpGet("roles")]
        [ProducesResponseType(typeof(MemberPermissionRoleDTO), StatusCodes.Status200OK)]

        public async Task<IActionResult> Roles()
        {

            var result = await _memberPermissionService.GetMemberPermissionRoleAsync();


            Log.Information("取得角色列表成功，總數: {Count}", result.Roles.Count());
            return Success(result, "Success", 200);
        }
        
    }
}
