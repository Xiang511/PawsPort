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
    public class MemberController : ApiControllerBase
    {
        private readonly MemberProfileService _memberProfileService;

        public MemberController(PetDbContext context, MemberProfileService memberProfileService)
        {
            _memberProfileService = memberProfileService;
        }



        /// <summary>
        /// 取得所有會員列表（未刪除）
        /// </summary>
        /// <returns>會員列表 JSON</returns>
        /// <response code="200">成功取得會員列表</response>
        [HttpGet]
        [ProducesResponseType(typeof(IEnumerable<UserTable>), StatusCodes.Status200OK)]
        public async Task<IActionResult> Members()
        {
            // 1. 使用 await 呼叫非同步版本的 Service 方法
            // 註：Service 層的方法通常需改為 GetAllUserInfoAsync()
            var users = await _memberProfileService.GetAllUserInfoAsync();

            // 2. 根據你的邏輯回傳結果
            // 如果是要回傳資料，應使用 Ok(users) 而非 NoContent()
            if (users == null || !users.Any())
            {
                return NoContent();
            }

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

            return Success(summary, "成功取得統計資訊", 200);
        }


        /// <summary>
        /// 更新指定會員資料
        /// </summary>
        /// <param name="id">會員ID</param>
        /// <param name="userDto">更新的會員資料</param>
        /// <returns>無內容</returns>
        /// <response code="204">成功更新會員</response>
        /// <response code="400">會員ID不一致或資料格式錯誤</response>
        [HttpPut("{id}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> UpdateMembers(int id, MemberUserDTO userDto)
        {
            // 驗證路由中的 id 與 DTO 中的 UserId 是否一致
            if (id != userDto.UserId)
            {
                return Failure("USER_ID_MISMATCH", "會員ID不一致", 400);
            }

            var result = await _memberProfileService.UpdateUserInfoAsync(userDto);

            if (result)
            {
                Log.Information("更新會員成功 會員ID:{UserId} 名稱:{Name}", userDto.UserId, userDto.Name);
            }
            else
            {
                Log.Warning("更新會員失敗 會員ID:{UserId}", userDto.UserId);
            }

            return NoContent();
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
        public async Task<IActionResult> Delete(string id)
        {
            // 手動驗證 id 是否為空
            if (string.IsNullOrWhiteSpace(id))
                return Failure("USER_ID_EMPTY", "會員ID不能為空", 400);

            // 手動解析並驗證 id 是否為有效整數
            if (!int.TryParse(id, out int memberId))
                return Failure("USER_ID_INVALID", "會員ID格式錯誤，必須是數字", 400);
            var result = await _memberProfileService.DeleteUserAsync(memberId);

            if (result)
            {
                Log.Information("刪除會員成功 會員ID:{UserId}", memberId);
                return NoContent();
            }
            else
            {
                Log.Warning("刪除會員失敗 會員ID:{UserId}", memberId);
                return Failure("USER_NOT_FOUND", "找不到使用者", 404);
            }
        }

        /// <summary>
        /// 測試例外處理機制（僅供開發測試）
        /// </summary>
        /// <returns>拋出例外</returns>
        /// <response code="500">內部伺服器錯誤</response>
        [HttpGet("throw")]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> ThrowError()
        {
            // 模擬一個非預期的噴錯
            throw new Exception("這是手動觸發的測試例外");
        }
    }
}
