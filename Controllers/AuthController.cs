using Microsoft.AspNetCore.Authorization;
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
        private readonly LoginLogService _loginLogService;

        public AuthController(
            AuthService authService,
            MemberProfileService memberProfileService,
            LoginLogService loginLogService)
        {
            _authService = authService;
            _memberProfileService = memberProfileService;
            _loginLogService = loginLogService;
        }


        /// <summary>
        /// 取得所有成功的登入記錄
        /// </summary>
        /// <param name="page">頁碼（從 1 開始）</param>
        /// <param name="pageSize">每頁筆數（預設 50，最大 200）</param>
        /// <returns>成功的登入記錄列表</returns>
        /// <response code="200">查詢成功</response>
        [Authorize(Policy = "會員系統_系統管理員")]
        [HttpGet("logins/successful")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [Tags("登入記錄")]
        public async Task<IActionResult> GetAllSuccessfulLogins([FromQuery] int page = 1, [FromQuery] int pageSize = 50)
        {
            try
            {
                // 限制 pageSize 最大值
                pageSize = Math.Min(pageSize, 200);
                var skip = (page - 1) * pageSize;

                var logs = await _loginLogService.GetAllSuccessfulLoginsAsync(skip, pageSize);
                var totalCount = await _loginLogService.GetSuccessfulLoginsCountAsync();

                var result = new
                {
                    Data = logs,
                    Pagination = new
                    {
                        CurrentPage = page,
                        PageSize = pageSize,
                        TotalCount = totalCount,
                        TotalPages = (int)Math.Ceiling(totalCount / (double)pageSize)
                    }
                };

                Log.Debug("[AuthController] GetAllSuccessfulLogins - 查詢成功，共 {Count} 筆", logs.Count);
                return Success(result);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "[AuthController] GetAllSuccessfulLogins - 查詢失敗");
                return Failure("查詢失敗", "Query_Error", 500);
            }
        }

        /// <summary>
        /// 取得所有失敗的登入記錄
        /// </summary>
        /// <param name="page">頁碼（從 1 開始）</param>
        /// <param name="pageSize">每頁筆數（預設 50，最大 200）</param>
        /// <returns>失敗的登入記錄列表</returns>
        /// <response code="200">查詢成功</response>
        [Authorize(Policy = "會員系統_系統管理員")]
        [HttpGet("logins/failed")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [Tags("登入記錄")]
        public async Task<IActionResult> GetAllFailedLogins([FromQuery] int page = 1, [FromQuery] int pageSize = 50)
        {
            try
            {
                // 限制 pageSize 最大值
                pageSize = Math.Min(pageSize, 200);
                var skip = (page - 1) * pageSize;

                var logs = await _loginLogService.GetAllFailedLoginsAsync(skip, pageSize);
                var totalCount = await _loginLogService.GetFailedLoginsCountAsync();

                var result = new
                {
                    Data = logs,
                    Pagination = new
                    {
                        CurrentPage = page,
                        PageSize = pageSize,
                        TotalCount = totalCount,
                        TotalPages = (int)Math.Ceiling(totalCount / (double)pageSize)
                    }
                };

                Log.Debug("[AuthController] GetAllFailedLogins - 查詢成功，共 {Count} 筆", logs.Count);
                return Success(result);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "[AuthController] GetAllFailedLogins - 查詢失敗");
                return Failure("查詢失敗", "Query_Error", 500);
            }
        }

        /// <summary>
        /// 取得使用者的登入歷史記錄
        /// </summary>
        /// <param name="userId">使用者 ID</param>
        /// <param name="limit">限制筆數（預設10筆）</param>
        /// <returns>登入歷史記錄列表</returns>
        /// <response code="200">成功取得登入歷史</response>
        [Authorize(Policy = "會員系統_系統管理員")]
        [HttpGet("login-history/{userId}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [Tags("身分驗證")]
        public async Task<IActionResult> GetLoginHistory(int userId, [FromQuery] int limit = 10)
        {
            Log.Debug("[AuthController] GetLoginHistory - UserId: {UserId}, Limit: {Limit}", userId, limit);

            var loginHistory = await _loginLogService.GetUserLoginHistoryAsync(userId, limit);

            return Success(loginHistory, "成功取得登入歷史", 200);
        }

        /// <summary>
        /// 取得最近的失敗登入嘗試
        /// </summary>
        /// <param name="email">使用者 Email</param>
        /// <param name="hours">時間範圍（小時，預設24小時）</param>
        /// <returns>失敗登入記錄列表</returns>
        /// <response code="200">成功取得失敗登入記錄</response>
        [Authorize(Policy = "會員系統_系統管理員")]
        [HttpGet("failed-logins")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [Tags("身分驗證")]
        public async Task<IActionResult> GetFailedLogins([FromQuery] string email, [FromQuery] int hours = 24)
        {
            Log.Debug("[AuthController] GetFailedLogins - Email: {Email}, Hours: {Hours}", email, hours);

            var timeWindow = TimeSpan.FromHours(hours);
            var failedLogins = await _loginLogService.GetRecentFailedLoginsAsync(email, timeWindow);

            return Success(failedLogins, "成功取得失敗登入記錄", 200);
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
            // 取得客戶端 IP 和 User Agent
            var ipAddress = GetClientIpAddress();
            var userAgent = Request.Headers["User-Agent"].ToString();

            if (model.UserEmail == null)
            {
                // 記錄失敗的登入嘗試
                await _loginLogService.LogFailedLoginAsync(
                    model.UserEmail ?? "unknown",
                    ipAddress,
                    userAgent,
                    "使用者不存在");

                return Failure("使用者不存在", "User_Not_Found", 404);
            }

            var userInfo = await _memberProfileService.GetUserInfoByEmailAsync(model.UserEmail);
            var token = await _authService.ValidateUser(model.UserEmail, model.Password);

            if (!string.IsNullOrEmpty(token))
            {
                Response.Cookies.Append("X-Access-Token", token, new CookieOptions
                {
                    HttpOnly = true,
                    Secure = Request.IsHttps,
                    SameSite = SameSiteMode.Lax,
                    Path = "/",
                    Expires = DateTimeOffset.UtcNow.AddHours(2)
                });

                // 記錄成功的登入
                await _loginLogService.LogUserLoginAsync(
                    userInfo.UserId,
                    model.UserEmail,
                    ipAddress,
                    userAgent);

                Log.Debug("[AuthController] Login - 成功登入, Email: {Email}, IP: {IP}", model.UserEmail, ipAddress);
                return Success(new { Token = token, User = userInfo });
            }
            else
            {
                // 記錄失敗的登入嘗試
                await _loginLogService.LogFailedLoginAsync(
                    model.UserEmail,
                    ipAddress,
                    userAgent,
                    "帳號或密碼錯誤");

                return Failure("帳號或密碼錯誤", "Invalid_Credentials", 401);
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

        public async Task<IActionResult> Logout()
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
            return Success(true, "登出成功", 200);
        }       
        /// <summary>
        /// 取得客戶端真實 IP 位址
        /// </summary>
        private string GetClientIpAddress()
        {
            // 依序檢查可能包含真實 IP 的 Header
            var forwardedFor = Request.Headers["X-Forwarded-For"].FirstOrDefault();
            if (!string.IsNullOrEmpty(forwardedFor))
            {
                // X-Forwarded-For 可能包含多個 IP，取第一個
                return forwardedFor.Split(',').FirstOrDefault()?.Trim() ?? Request.HttpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown";
            }

            var realIp = Request.Headers["X-Real-IP"].FirstOrDefault();
            if (!string.IsNullOrEmpty(realIp))
            {
                return realIp;
            }

            // 如果沒有 Proxy Header，直接取連線 IP
            return Request.HttpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown";
        }

    }
}

