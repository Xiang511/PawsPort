using Microsoft.AspNetCore.Mvc;
using PawsPort.Dtos;
using PawsPort.Models;
using PawsPort.Services;
using Serilog;
using static Microsoft.CodeAnalysis.CSharp.SyntaxTokenParser;

namespace PawsPort.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Produces("application/json")]
    public class UsersController : ApiControllerBase
    {
        private readonly MemberProfileService _memberProfileService;

        public UsersController(PetDbContext context, MemberProfileService memberProfileService)
        {
            _memberProfileService = memberProfileService;
        }

        /// <summary>
        /// 取得所有會員列表
        /// </summary>
        /// <returns>會員列表 JSON</returns>
        /// <response code="200">成功取得會員列表</response>
        [HttpGet]
        [ProducesResponseType(typeof(IEnumerable<UserTable>), StatusCodes.Status200OK)]
        public async Task<IActionResult> Members()
        {

            var users = await _memberProfileService.GetAllUserInfoAsync();
            Log.Information("取得會員資料成功 共{user}筆", users.Count);
            return Success(users, "Success", 200);
        }

        /// <summary>
        /// 取得單一會員資訊
        /// </summary>
        /// <returns>會員列表 JSON</returns>
        /// <response code="200">成功取得會員列表</response>
        [HttpGet("{id}")]
        [ProducesResponseType(typeof(IEnumerable<UserTable>), StatusCodes.Status200OK)]
        public async Task<IActionResult> Members(int? id)
        {
            bool exist = await _memberProfileService.CheckUserInfoAsync(id,null);

            if (!exist)
            {
                return Failure("USER_NOT_FOUND", "找不到使用者", 404);
            }

            var users = await _memberProfileService.GetUserInfoByIdAsync(id);
            Log.Information("取得會員資料成功 Id:{users.UserId} 名稱:{users.Name}", users.UserId, users.Name);
            return Success(users, "Success", 200);
        }



        /// <summary>
        /// 創建新會員
        /// </summary>
        /// <param name="user">會員資料</param>
        /// <returns>創建的會員資料 JSON</returns>
        /// <response code="200">成功創建會員</response>
        [HttpPost]
        [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
        public async Task<IActionResult> Members(MemberUserDTO user)
        {

            var result = await _memberProfileService.CreateUserAsync(user);

            Log.Information("創建會員成功 名稱{Name}", result.Name);

            return Success(result, "創建會員成功", 200);
        }

        /// <summary>
        /// 取得會員統計摘要資訊
        /// </summary>
        /// <returns>會員統計資料 JSON</returns>
        /// <response code="200">成功取得統計資訊</response>
        [HttpGet("Summary")]
        [ProducesResponseType(typeof(IEnumerable<UserTable>), StatusCodes.Status200OK)]
        public async Task<IActionResult> Summary()
        {
            // 取得會員統計資訊
            var summary = await _memberProfileService.GetMemberSummaryAsync();

            Log.Information("成功取得統計資訊 會員總數{summary.MemberCount},月註冊數{summary.MemberMonthSignUp},認證比例{summary.VerifyPercentage},訂閱電子報人數{summary.SubscribedMemberCount}", summary.MemberCount,
                summary.MemberMonthSignUp,
                summary.VerifyPercentage,
                summary.SubscribedMemberCount);

            return Success(summary, "成功取得統計資訊", 200);
        }


        /// <summary>
        /// 更新指定會員資料
        /// </summary>
        /// <param name="id">會員ID</param>
        /// <param name="userDto">更新的會員資料</param>
        /// <returns>無內容</returns>
        /// <response code="200">成功更新會員</response>
        /// <response code="400">會員ID不一致或資料格式錯誤</response>
        [HttpPut("{id}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> UpdateMembers(int? id, MemberUserDTO userDto)
        {

            if (id != userDto.UserId)
            {
                return Failure("USER_ID_MISMATCH", "會員ID不一致", 400);
            }

            bool exist = await _memberProfileService.CheckUserInfoAsync(id,null);

            if (!exist)
            {
                return Failure("USER_NOT_FOUND", "找不到使用者", 404);
            }

            var result = await _memberProfileService.UpdateUserInfoAsync(id.Value, userDto);

            Log.Information("更新會員成功 會員ID:{UserId} 名稱:{Name}", userDto.UserId, userDto.Name);
            return Success(result, "Success", 200);

        }

        /// <summary>
        /// 刪除指定會員（軟刪除）
        /// </summary>
        /// <param name="id">會員ID</param>
        /// <returns>無內容或錯誤訊息</returns>
        /// <response code="204">成功刪除會員</response>
        /// <response code="400">會員ID格式錯誤</response>
        /// <response code="404">找不到指定會員</response>
        [HttpDelete("{id}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Delete(int? id)
        {
            
            var result = await _memberProfileService.DeleteUserAsync(id);

            if (result)
            {
                Log.Information("刪除會員成功 會員ID:{UserId}", id);
                return NoContent();
            }
            else
            {
                Log.Warning("刪除會員失敗 會員ID:{UserId}", id);
                return Failure("USER_NOT_FOUND", "找不到使用者", 404);
            }
        }
    }
}
