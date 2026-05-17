using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PawsPort.Dtos;
using PawsPort.Models;
using PawsPort.Services;
using Serilog;
using static Microsoft.CodeAnalysis.CSharp.SyntaxTokenParser;

namespace PawsPort.Controllers
{
    [Authorize(Policy = "會員系統_普通管理員")]
    [ApiController]
    [Route("api/[controller]")]
    [Produces("application/json")]

    public class UsersController : ApiControllerBase
    {
        private readonly MemberProfileService _memberProfileService;
        private readonly MemberPermissionService _memberPermissionService;

        public UsersController(PetDbContext context, MemberProfileService memberProfileService, MemberPermissionService memberPermissionService)
        {
            _memberProfileService = memberProfileService;
            _memberPermissionService = memberPermissionService;
        }

        /// <summary>
        /// 取得所有會員列表
        /// </summary>
        /// <returns>會員列表 JSON</returns>
        /// <response code="200">成功取得會員列表</response>
        
        [HttpGet]
        [ProducesResponseType(typeof(List<MemberUserDTO>), StatusCodes.Status200OK)]
        [Tags("會員管理")]
        public async Task<IActionResult> Members()
        {
            Log.Debug("[UsersController] Members GET - Entry");

            Log.Debug("[UsersController] 調用 MemberProfileService.GetAllUserInfoAsync");
            var users = await _memberProfileService.GetAllUserInfoAsync();
            Log.Debug("[UsersController] 取得會員資料成功, 共 {Count} 筆", users.Count);
            return Success(users, "Success", 200);
        }

        /// <summary>
        /// 取得指定會員資訊
        /// </summary>
        /// <param name="id">會員 ID</param>
        /// <returns>會員詳細資料</returns>
        /// <response code="200">成功取得會員資料</response>
        /// <response code="404">找不到指定的會員</response>
        
        [HttpGet("{id}")]
        [ProducesResponseType(typeof(MemberUserDTO), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [Tags("會員管理")]
        public async Task<IActionResult> Members(int? id)
        {
            Log.Debug("[UsersController] Members GET by id - Entry, UserId: {UserId}", id);

            Log.Debug("[UsersController] 調用 MemberProfileService.CheckUserInfoAsync, UserId: {UserId}", id);
            bool exist = await _memberProfileService.CheckUserInfoAsync(id,null);

            if (!exist)
            {
                Log.Warning("[UsersController] 會員不存在, UserId: {UserId}", id);
                return Failure("USER_NOT_FOUND", "找不到使用者", 404);
            }

            Log.Debug("[UsersController] 調用 MemberProfileService.GetUserInfoByIdAsync, UserId: {UserId}", id);
            var users = await _memberProfileService.GetUserInfoByIdAsync(id);
            Log.Debug("[UsersController] 取得會員資料成功, UserId: {UserId}, 名稱: {Name}", users.UserId, users.Name);
            return Success(users, "Success", 200);
        }



        /// <summary>
        /// 創建新會員
        /// </summary>
        /// <param name="createDto">會員註冊資料（UserId 會由系統自動生成）</param>
        /// <returns>創建成功的會員資料（包含自動生成的 UserId）</returns>
        /// <response code="200">成功創建會員</response>
        /// <response code="400">請求資料格式錯誤或必填欄位缺失</response>

        [HttpPost]
        [ProducesResponseType(typeof(MemberUserDTO), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [Tags("會員管理")]
        public async Task<IActionResult> Members(CreateMemberDTO user)
        {
            Log.Debug("[UsersController] Members POST - Entry, 會員名稱: {Name}", user.Name);

            Log.Debug("[UsersController] 調用 MemberProfileService.CreateUserAsync, 會員名稱: {Name}", user.Name);
            var result = await _memberProfileService.CreateUserAsync(user);

            Log.Debug("[UsersController] 創建會員成功, 名稱: {Name}", result.Name);

            return Success(result, "創建會員成功", 200);
        }

        /// <summary>
        /// 取得會員統計資訊（儀表板用）
        /// </summary>
        /// <returns>會員統計數據，包含總數、月註冊數、認證比例、訂閱數等</returns>
        /// <response code="200">成功取得統計資訊</response>
        
        [HttpGet("Summary")]
        [ProducesResponseType(typeof(MemberSummaryDTO), StatusCodes.Status200OK)]
        [Tags("會員管理")]
        public async Task<IActionResult> Summary()
        {
            Log.Debug("[UsersController] Summary GET - Entry");

            // 取得會員統計資訊
            Log.Debug("[UsersController] 調用 MemberProfileService.GetMemberSummaryAsync");
            var summary = await _memberProfileService.GetMemberSummaryAsync();

            Log.Debug("[UsersController] 成功取得統計資訊, 會員總數: {MemberCount}, 月註冊數: {MonthSignUp}, 認證比例: {VerifyPercentage}, 訂閱電子報人數: {SubscribedCount}", 
                summary.MemberCount,
                summary.MemberMonthSignUp,
                summary.VerifyPercentage,
                summary.SubscribedMemberCount);

            return Success(summary, "成功取得統計資訊", 200);
        }


        /// <summary>
        /// 更新指定會員資訊
        /// </summary>
        /// <param name="id">會員ID</param>
        /// <param name="userDto">更新的會員資料</param>
        /// <returns>更新後的會員資料</returns>
        /// <response code="200">成功更新會員</response>
        /// <response code="400">會員 ID 不一致或資料格式錯誤</response>
        /// <response code="404">找不到指定的會員</response>
        
        [HttpPut("{id}")]
        [ProducesResponseType(typeof(MemberUserDTO), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [Tags("會員管理")]
        public async Task<IActionResult> UpdateMembers(int? id, MemberUserDTO userDto)
        {
            Log.Debug("[UsersController] UpdateMembers PUT - Entry, UserId: {UserId}, 會員名稱: {Name}", id, userDto.Name);

            if (id != userDto.UserId)
            {
                Log.Warning("[UsersController] 會員ID不一致, 路徑ID: {PathId}, DTO ID: {DtoId}", id, userDto.UserId);
                return Failure("USER_ID_MISMATCH", "會員ID不一致", 400);
            }

            Log.Debug("[UsersController] 調用 MemberProfileService.CheckUserInfoAsync, UserId: {UserId}", id);
            bool exist = await _memberProfileService.CheckUserInfoAsync(id,null);

            if (!exist)
            {
                Log.Warning("[UsersController] 會員不存在, UserId: {UserId}", id);
                return Failure("USER_NOT_FOUND", "找不到使用者", 404);
            }

            Log.Debug("[UsersController] 調用 MemberProfileService.UpdateUserInfoAsync, UserId: {UserId}", id);
            var result = await _memberProfileService.UpdateUserInfoAsync(id.Value, userDto);

            Log.Debug("[UsersController] 更新會員成功, 會員ID: {UserId}, 名稱: {Name}", userDto.UserId, userDto.Name);
            return Success(result, "Success", 200);

        }

        /// <summary>
        /// 刪除指定會員（軟刪除）
        /// </summary>
        /// <param name="id">會員 ID</param>
        /// <returns>無內容（204）或錯誤訊息</returns>
        /// <response code="204">成功刪除會員（軟刪除）</response>
        /// <response code="404">找不到指定會員</response>
        /// 
        
        [HttpPatch("{id}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [Tags("會員管理")]
        public async Task<IActionResult> DeleteMember(int? id)
        {
            Log.Debug("[UsersController] DeleteMember DELETE - Entry, UserId: {UserId}", id);

            Log.Debug("[UsersController] 調用 MemberProfileService.DeleteUserAsync, UserId: {UserId}", id);
            var result = await _memberProfileService.DeleteUserAsync(id);

            if (result)
            {
                Log.Debug("[UsersController] 刪除會員成功, 會員ID: {UserId}", id);
                return NoContent();
            }
            else
            {
                Log.Warning("[UsersController] 刪除會員失敗, 會員ID: {UserId}", id);
                return Failure("USER_NOT_FOUND", "找不到使用者", 404);
            }
        }

        /// <summary>
        /// 取得指定會員的權限列表
        /// </summary>
        /// <param name="id">使用者 ID</param>
        /// <returns>使用者的權限角色列表</returns>
        /// <response code="200">成功取得使用者權限</response>
        /// <response code="404">找不到指定的使用者</response>
        [Authorize(Policy = "會員系統_系統管理員")]

        [HttpGet("/api/users/{id}/roles")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [Tags("會員管理")]
        public async Task<IActionResult> QueryRoleById(int? id)
        {
            Log.Debug("[UsersController] QueryRoleById GET - Entry, UserId: {UserId}", id);

            Log.Debug("[UsersController] 調用 MemberPermissionService.GetUserPermissionsAsync, UserId: {UserId}", id);
            var userPermission = await _memberPermissionService.GetUserPermissionsAsync(id);

            if (userPermission == null)
            {
                Log.Warning("[UsersController] 找不到指定的使用者權限, UserId: {UserId}", id);
                return Failure("PERMISSION_NOT_FOUND", "找不到指定的使用者", 404);
            }

            Log.Debug("[UsersController] 成功取得使用者權限, UserId: {UserId}", id);
            return Success(userPermission, "Success", 200);
        }



        /// <summary>
        /// 新增會員權限
        /// </summary>
        /// <param name="US">使用者權限資料（包含 UserId、SystemId、RoleId）</param>
        /// <returns>操作結果 JSON</returns>
        /// <response code="200">成功新增使用者權限</response>
        /// <response code="204">新增失敗（可能因為權限已存在或參數無效）</response>
        /// <response code="409">權限衝突（該使用者已擁有此系統的相同角色權限）</response>
        [Authorize(Policy = "會員系統_系統管理員")]
        [HttpPost("/api/users/roles")]
        [ProducesResponseType(typeof(bool), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        [Tags("會員管理")]
        public async Task<IActionResult> CreateRole(MemberUserSystemRoleDTO US)
        {
            Log.Debug("[UsersController] CreateRole POST - Entry, UserId: {UserId}, SystemId: {SystemId}, RoleId: {RoleId}", US.UserId, US.SystemId, US.RoleId);

            Log.Debug("[UsersController] 調用 MemberPermissionService.CheckMemberPermissionExistAsync");
            bool exist = await _memberPermissionService.CheckMemberPermissionExistAsync(US);

            if (exist)
            {
                Log.Warning("[UsersController] 嘗試新增已存在的使用者權限: UserId={UserId}, SystemId={SystemId}, RoleId={RoleId}", US.UserId, US.SystemId, US.RoleId);
                return Failure("PERMISSION_ALREADY_EXISTS", "該使用者已擁有此系統的相同角色權限", 409);
            }

            Log.Debug("[UsersController] 調用 MemberPermissionService.CreateMemberPermissionAsync");
            var result = await _memberPermissionService.CreateMemberPermissionAsync(US);

            if (result == false)
            {
                Log.Warning("[UsersController] 新增使用者權限失敗: UserId={UserId}, SystemId={SystemId}, RoleId={RoleId}", US.UserId, US.SystemId, US.RoleId);
                return NoContent();
            }

            Log.Debug("[UsersController] 成功新增使用者權限: UserId={UserId}, SystemId={SystemId}, RoleId={RoleId}", US.UserId, US.SystemId, US.RoleId);
            return Success(result, "Success", 200);
        }

        /// <summary>
        /// 更新使用者權限
        /// </summary>
        /// <param name="mappingId">權限對應 ID</param>
        /// <param name="user">更新的權限資料（包含 UserId、SystemId、RoleId）</param>
        /// <returns>操作結果</returns>
        /// <response code="204">成功更新使用者權限</response>
        /// <response code="400">無效的權限對應 ID（ID 為空或小於 0）</response>
        /// <response code="409">權限衝突或權限不存在</response>
        [Authorize(Policy = "會員系統_系統管理員")]

        [HttpPatch("/api/users/{mappingId}/roles")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        [Tags("會員管理")]

        public async Task<IActionResult> EditRole(int? mappingId, MemberPermissionUpdateRoleDTO user)
        {
            Log.Debug("[UsersController] EditRole PATCH - Entry, MappingId: {MappingId}, UserId: {UserId}, SystemId: {SystemId}, RoleId: {RoleId}", mappingId, user.UserId, user.SystemId, user.RoleId);

            // 驗證路由參數：mappingId 不可為 null 或小於 0
            if (mappingId == null || mappingId < 0)
            {
                Log.Warning("[UsersController] 無效的權限對應 ID: {MappingId}", mappingId);
                return Failure("INVALID_MAPPING_ID", "無效ID", 400);
            }

            Log.Debug("[UsersController] 調用 MemberPermissionService.CheckMemberPermissionExistAsync");
            bool exist = await _memberPermissionService.CheckMemberPermissionExistAsync(user);

            if (exist)
            {
                Log.Warning("[UsersController] 嘗試更新為已存在的使用者權限: UserId={UserId}, SystemId={SystemId}, RoleId={RoleId}", user.UserId, user.SystemId, user.RoleId);
                return Failure("PERMISSION_ALREADY_EXISTS", "該使用者已擁有相同角色權限", 409);
            }

            Log.Debug("[UsersController] 調用 MemberPermissionService.UpdateMemberPermissionRoleAsync, MappingId: {MappingId}", mappingId);
            bool result = await _memberPermissionService.UpdateMemberPermissionRoleAsync(mappingId, user);


            if (!result)
            {
                Log.Warning("[UsersController] 找不到指定的使用者權限: MappingId={MappingId}", mappingId);
                return Failure("User_NOT_FOUND", "找不到指定的Id或是使用者", 404);
            }

            Log.Debug("[UsersController] 成功更新使用者權限: MappingId={MappingId}, UserId={UserId}, SystemId={SystemId}, RoleId={RoleId}", mappingId, user.UserId, user.SystemId, user.RoleId);
            return NoContent();

        }

        /// <summary>
        /// 刪除使用者權限
        /// </summary>
        /// <param name="mappingId">權限對應 ID</param>
        /// <returns>操作結果</returns>
        /// <response code="204">成功刪除使用者權限</response>
        /// <response code="400">無效的權限對應 ID（ID 為空）</response>
        /// <response code="404">找不到指定的權限</response>
        [Authorize(Policy = "會員系統_系統管理員")]

        [HttpDelete("/api/users/{mappingId}/roles")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [Tags("會員管理")]
        public async Task<IActionResult> DeleteRole(int? mappingId)
        {
            Log.Debug("[UsersController] DeleteRole DELETE - Entry, MappingId: {MappingId}", mappingId);

            // 驗證路由參數：mappingId 不可為 null
            if (mappingId == null)
            {
                Log.Warning("[UsersController] 刪除使用者權限失敗，無效的權限對應 ID: {MappingId}", mappingId);
                return Failure("INVALID_MAPPING_ID", "無效ID", 400);
            }

            Log.Debug("[UsersController] 調用 MemberPermissionService.DeleteMemberPermissionRoleAsync, MappingId: {MappingId}", mappingId);
            bool result = await _memberPermissionService.DeleteMemberPermissionRoleAsync(mappingId);

            if (!result)
            {
                Log.Warning("[UsersController] 找不到指定的使用者權限，刪除失敗: MappingId={MappingId}", mappingId);
                return Failure("PERMISSION_NOT_FOUND", "找不到指定的權限", 404);
            }

            Log.Debug("[UsersController] 成功刪除使用者權限: MappingId={MappingId}", mappingId);
            return NoContent();
        }
    }
}
