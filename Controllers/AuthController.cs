using Microsoft.AspNetCore.Mvc;
using PawsPort.Dtos;
using PawsPort.Services;
using Serilog;

namespace PawsPort.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Produces("application/json")]
    public class AuthController : ApiControllerBase
    {
        private readonly AuthService _authService;
        private readonly MemberProfileService _memberProfileService;
        public AuthController(AuthService authService, MemberProfileService memberProfileService)
        {
            _authService = authService;
            _memberProfileService = memberProfileService;
        }

        /// <summary>
        /// 使用者登入
        /// </summary>
        /// <param name="model">登入資訊，包含 Email 和密碼</param>
        /// <returns>登入成功返回 Token 和使用者資訊</returns>
        /// <response code="200">登入成功</response>
        /// <response code="401">帳號或密碼錯誤</response>
        /// <response code="404">使用者不存在</response>
        [HttpPost("login")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [Tags("身分驗證")]
        public async Task<IActionResult> Login([FromBody] LoginDTO model)
        {
            if (model.UserEmail == null)
            {
                return Failure("使用者不存在", "User_Not_Found", 404);
            }
            else
            {
                var userInfo = await _memberProfileService.GetUserInfoByEmailAsync(model.UserEmail);
                var token = await _authService.ValidateUser(model.UserEmail, model.Password);

                if (!string.IsNullOrEmpty(token))
                {
                    Response.Cookies.Append("X-Access-Token", token, new CookieOptions
                    {
                        HttpOnly = true, // 前端 JavaScript 無法讀取，防範 XSS
                        Secure = Request.IsHttps, // 根據當前請求協議決定（生產環境應該是 HTTPS）
                        SameSite = SameSiteMode.Lax, // 防範 CSRF 攻擊
                        Path = "/",
                        Expires = DateTimeOffset.UtcNow.AddHours(2)
                    });

                    Log.Debug("[AuthController] Login - 成功登入, Email: {Email}", model.UserEmail);
                    return Success(new { Token = token, User = userInfo });
                }
                else
                {
                    return Failure("帳號或密碼錯誤", "Invalid_Credentials", 401);
                }

            }

        }
        /// <summary>
        /// 使用者登出
        /// </summary>
        /// <returns>登出成功訊息</returns>
        /// <response code="200">登出成功</response>
        [HttpPost("logout")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [Tags("身分驗證")]

        public IActionResult Logout()
        {
            Log.Debug("[AuthController] Logout POST - Entry");

            // 檢查 Cookie 是否存在
            if (Request.Cookies.ContainsKey("X-Access-Token"))
            {
                // 建立一個與登入時完全相同的設定檔（除了過期時間）
                var cookieOptions = new CookieOptions
                {
                    HttpOnly = true,
                    Secure = Request.IsHttps, // 根據當前請求協議決定
                    SameSite = SameSiteMode.Lax,
                    Path = "/",
                    Expires = DateTimeOffset.UtcNow.AddDays(-1)
                };

                // 刪除 Cookie
                Response.Cookies.Delete("X-Access-Token", cookieOptions);
                Log.Debug("[AuthController] Logout - Cookie 已刪除");
            }

            Log.Debug("[AuthController] Logout POST - Exit");
            return Success("登出成功");
        }
        /// <summary>
        /// 使用者註冊
        /// </summary>
        /// <param name="model">註冊資訊，包含 Email、姓名和密碼</param>
        /// <returns>註冊成功訊息</returns>
        /// <response code="200">註冊成功</response>
        /// <response code="400">Email 已存在</response>
        /// <response code="500">註冊失敗</response>
        [HttpPost("register")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [Tags("身分驗證")]

        public async Task<IActionResult> Register([FromBody] UserRegisterDTO model)
        {
            Log.Debug("[AuthController] Register POST - Entry, Email: {Email}, Name: {Name}", model.Email, model.Name);

            // 檢查 Email 是否已存在
            var isEmailExist = await _memberProfileService.GetUserInfoByEmailAsync(model.Email);

            if (isEmailExist != null)
            {
                Log.Warning("[AuthController] Register - Email 已存在: {Email}", model.Email);
                return Failure("EMAIL_EXISTS", "Email 已存在", 400);
            }

            // 呼叫 RegisterUser（密碼雜湊在 Service 中處理）
            var result = await _authService.RegisterUser(model);

            if (result)
            {
                Log.Debug("[AuthController] Register - 註冊成功, Email: {Email}", model.Email);
                return Success(new { Message = "註冊成功", Email = model.Email });
            }
            else
            {
                Log.Warning("[AuthController] Register - 註冊失敗, Email: {Email}", model.Email);
                return Failure("REGISTRATION_FAILED", "註冊失敗，請稍後再試", 500);
            }
        }
    }
}

