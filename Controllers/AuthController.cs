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
        private readonly MemberPermissionService _memberPermissionService;
        private readonly GoogleOAuthService _googleOAuthService;

        public AuthController(
            AuthService authService,
            MemberProfileService memberProfileService,
            LoginLogService loginLogService, 
            MemberPermissionService memberPermissionService,
            GoogleOAuthService googleOAuthService)
        {
            _authService = authService;
            _memberProfileService = memberProfileService;
            _loginLogService = loginLogService;
            _memberPermissionService = memberPermissionService;
            _googleOAuthService = googleOAuthService;
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
        /// 使用者註冊 - 步驟1：送出註冊資料，發送 Email 驗證碼
        /// </summary>
        /// <param name="model">註冊資訊，包含 Email、姓名和密碼</param>
        /// <returns>驗證碼已發送訊息</returns>
        /// <response code="200">驗證碼已發送，請至信箱完成驗證</response>
        /// <response code="400">Email 已存在或輸入有誤</response>
        /// <response code="500">系統錯誤</response>
        [HttpPost("register")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [Tags("身分驗證")]
        public async Task<IActionResult> Register([FromBody] UserRegisterDTO model)
        {
            Log.Debug("[AuthController] Register POST - Entry, Email: {Email}, Name: {Name}", model.Email, model.Name);

            var (success, message) = await _authService.RegisterUser(model);

            if (success)
            {
                Log.Debug("[AuthController] Register - 驗證碼已發送: {Email}", model.Email);
                return Success(new { Message = message, Email = model.Email });
            }
            else
            {
                Log.Warning("[AuthController] Register - 註冊失敗: {Email}, 原因: {Message}", model.Email, message);
                return Failure(message, "REGISTRATION_FAILED", 400);
            }
        }

        /// <summary>
        /// 使用者註冊 - 步驟2：驗證 Email 驗證碼，完成帳號建立
        /// </summary>
        /// <param name="model">Email 和驗證碼</param>
        /// <returns>註冊成功訊息</returns>
        /// <response code="200">Email 驗證成功，帳號已啟用</response>
        /// <response code="400">驗證碼錯誤或已過期</response>
        [HttpPost("register/verify-email")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [Tags("身分驗證")]
        public async Task<IActionResult> VerifyRegisterEmail([FromBody] RegisterVerifyDTO model)
        {
            Log.Debug("[AuthController] VerifyRegisterEmail POST - Email: {Email}", model.Email);

            if (string.IsNullOrWhiteSpace(model.Email) || string.IsNullOrWhiteSpace(model.VerificationCode))
            {
                return Failure("Email 和驗證碼不可為空", "INVALID_INPUT", 400);
            }

            var (success, message, userId) = await _authService.VerifyRegisterEmailAsync(model.Email, model.VerificationCode);

            if (success)
            {
                Log.Debug("[AuthController] VerifyRegisterEmail - 驗證成功: {Email}, UserId: {UserId}", model.Email, userId);

                // 為新用戶分配各系統一般成員權限
                // 寵物系統(2), 遊戲系統(3), 客服系統(4), 社群系統(5)
                var systemsToAssign = new[] { 2, 3, 4, 5 };
                var generalMemberRoleId = 3;

                foreach (var systemId in systemsToAssign)
                {
                    var permissionDto = new MemberUserSystemRoleDTO
                    {
                        UserId = userId,
                        SystemId = systemId,
                        RoleId = generalMemberRoleId
                    };

                    await _memberPermissionService.CreateMemberPermissionAsync(permissionDto);
                    Log.Debug("[AuthController] VerifyRegisterEmail - 已分配權限: UserId={UserId}, SystemId={SystemId}, RoleId={RoleId}",
                        userId, systemId, generalMemberRoleId);
                }

                return Success(new { Message = message, Email = model.Email, UserId = userId });
            }
            else
            {
                Log.Warning("[AuthController] VerifyRegisterEmail - 驗證失敗: {Email}, 原因: {Message}", model.Email, message);
                return Failure(message, "VERIFY_FAILED", 400);
            }
        }
        /// <summary>
        /// 使用者登入 - 步驟1：驗證帳號密碼並發送驗證碼
        /// </summary>
        /// <param name="model">登入資訊，包含 Email 和密碼</param>
        /// <returns>發送驗證碼成功</returns>
        /// <response code="200">驗證碼已發送</response>
        /// <response code="401">帳號或密碼錯誤</response>
        [HttpPost("login/request-code")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [Tags("身分驗證")]
        public async Task<IActionResult> RequestLoginVerificationCode([FromBody] RequestVerificationCodeDTO model)
        {
            var ipAddress = GetClientIpAddress();
            var userAgent = Request.Headers["User-Agent"].ToString();

            Log.Debug("[AuthController] RequestLoginVerificationCode - Email: {Email}, IP: {IP}", model.Email, ipAddress);

            if (string.IsNullOrWhiteSpace(model.Email))
            {
                await _loginLogService.LogFailedLoginAsync("unknown", ipAddress, userAgent, "Email 為空");
                return Failure("Email 不可為空", "Invalid_Input", 400);
            }

            // 調用服務發送驗證碼（若距上次成功登入 ≤ 20 分鐘，會直接回傳 token 跳過驗證）
            var (success, message, directToken, skipVerification) = await _authService.SendLoginVerificationCodeAsync(model.Email, model.Password);

            if (success && skipVerification)
            {
                // 距上次登入未超過 20 分鐘，直接完成登入
                var userInfo = await _memberProfileService.GetUserInfoByEmailAsync(model.Email);

                if (userInfo != null)
                {
                    Response.Cookies.Append("X-Access-Token", directToken, new CookieOptions
                    {
                        HttpOnly = true,
                        Secure = Request.IsHttps,
                        SameSite = SameSiteMode.Lax,
                        Path = "/",
                        Expires = DateTimeOffset.UtcNow.AddHours(2)
                    });

                    await _loginLogService.LogUserLoginAsync(userInfo.UserId, model.Email, ipAddress, userAgent);

                    Log.Information("[AuthController] RequestLoginVerificationCode - 跳過驗證直接登入成功: {Email}, IP: {IP}", model.Email, ipAddress);
                    return Success(new { Token = directToken, User = userInfo, Message = message, SkipVerification = true });
                }
            }

            if (success)
            {
                Log.Information("[AuthController] RequestLoginVerificationCode - 驗證碼已發送: {Email}", model.Email);
                return Success(new { Message = message, Email = model.Email, SkipVerification = false });
            }
            else
            {
                // 記錄失敗的登入嘗試
                await _loginLogService.LogFailedLoginAsync(model.Email, ipAddress, userAgent, message);
                Log.Warning("[AuthController] RequestLoginVerificationCode - 失敗: {Email}, 原因: {Message}", model.Email, message);
                return Failure(message, "Authentication_Failed", 401);
            }
        }

        /// <summary>
        /// 使用者登入 - 步驟2：驗證驗證碼並完成登入
        /// </summary>
        /// <param name="model">驗證碼驗證資訊</param>
        /// <returns>登入成功返回 Token 和使用者資訊</returns>
        /// <response code="200">登入成功</response>
        /// <response code="401">驗證碼錯誤或已過期</response>
        [HttpPost("login/verify-code")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [Tags("身分驗證")]
        public async Task<IActionResult> VerifyLoginCode([FromBody] VerifyCodeDTO model)
        {
            var ipAddress = GetClientIpAddress();
            var userAgent = Request.Headers["User-Agent"].ToString();

            Log.Debug("[AuthController] VerifyLoginCode - Email: {Email}, IP: {IP}", model.Email, ipAddress);

            if (string.IsNullOrWhiteSpace(model.Email) || string.IsNullOrWhiteSpace(model.VerificationCode))
            {
                return Failure("Email 和驗證碼不可為空", "Invalid_Input", 400);
            }

            // 驗證驗證碼
            var (success, token, message) = await _authService.VerifyLoginCodeAsync(model.Email, model.VerificationCode);

            if (success)
            {
                // 獲取使用者資訊
                var userInfo = await _memberProfileService.GetUserInfoByEmailAsync(model.Email);

                if (userInfo != null)
                {
                    // 設置 Cookie
                    Response.Cookies.Append("X-Access-Token", token, new CookieOptions
                    {
                        HttpOnly = true,
                        Secure = Request.IsHttps,
                        SameSite = SameSiteMode.Lax,
                        Path = "/",
                        Expires = DateTimeOffset.UtcNow.AddHours(2)
                    });

                    // 記錄成功的登入
                    await _loginLogService.LogUserLoginAsync(userInfo.UserId, model.Email, ipAddress, userAgent);

                    Log.Information("[AuthController] VerifyLoginCode - 登入成功: {Email}, IP: {IP}", model.Email, ipAddress);
                    return Success(new { Token = token, User = userInfo, Message = message });
                }
            }

            // 驗證失敗
            await _loginLogService.LogFailedLoginAsync(model.Email, ipAddress, userAgent, message);
            Log.Warning("[AuthController] VerifyLoginCode - 驗證失敗: {Email}, 原因: {Message}", model.Email, message);
            return Failure(message, "Verification_Failed", 401);
        }

        ///// <summary>
        ///// 使用者登入（舊方法，保留用於向後相容）
        ///// </summary>
        ///// <param name="model">登入資訊，包含 Email 和密碼</param>
        ///// <returns>登入成功返回 Token 和使用者資訊</returns>
        ///// <response code="200">登入成功</response>
        ///// <response code="401">帳號或密碼錯誤</response>
        ///// <response code="404">使用者不存在</response>
        //[HttpPost("login")]
        //[ProducesResponseType(StatusCodes.Status200OK)]
        //[ProducesResponseType(StatusCodes.Status401Unauthorized)]
        //[ProducesResponseType(StatusCodes.Status404NotFound)]
        //[Tags("身分驗證")]
        //public async Task<IActionResult> Login([FromBody] LoginDTO model)
        //{
        //    // 取得客戶端 IP 和 User Agent
        //    var ipAddress = GetClientIpAddress();
        //    var userAgent = Request.Headers["User-Agent"].ToString();

        //    // 驗證輸入
        //    if (string.IsNullOrWhiteSpace(model.UserEmail))
        //    {
        //        // 記錄失敗的登入嘗試（無論如何都要記錄）
        //        await _loginLogService.LogFailedLoginAsync(
        //            "unknown",
        //            ipAddress,
        //            userAgent,
        //            "Email 為空");

        //        return Failure("Email 不可為空", "Invalid_Input", 400);
        //    }

        //    // 查詢使用者資訊
        //    var userInfo = await _memberProfileService.GetUserInfoByEmailAsync(model.UserEmail);

        //    // 無論使用者是否存在，都嘗試驗證（避免時序攻擊）
        //    var token = await _authService.ValidateUser(model.UserEmail, model.Password);

        //    if (!string.IsNullOrEmpty(token) && userInfo != null)
        //    {
        //        // 登入成功
        //        Response.Cookies.Append("X-Access-Token", token, new CookieOptions
        //        {
        //            HttpOnly = true,
        //            Secure = Request.IsHttps,
        //            SameSite = SameSiteMode.Lax,
        //            Path = "/",
        //            Expires = DateTimeOffset.UtcNow.AddHours(2)
        //        });

        //        // 記錄成功的登入
        //        await _loginLogService.LogUserLoginAsync(
        //            userInfo.UserId,
        //            model.UserEmail,
        //            ipAddress,
        //            userAgent);

        //        Log.Debug("[AuthController] Login - 成功登入, Email: {Email}, IP: {IP}", model.UserEmail, ipAddress);
        //        return Success(new { Token = token, User = userInfo });
        //    }
        //    else
        //    {
        //        // 登入失敗（無論是帳號不存在還是密碼錯誤，都記錄）
        //        await _loginLogService.LogFailedLoginAsync(
        //            model.UserEmail,
        //            ipAddress,
        //            userAgent,
        //            "帳號或密碼錯誤");

        //        Log.Debug("[AuthController] Login - 登入失敗, Email: {Email}, IP: {IP}", model.UserEmail, ipAddress);

        //        // 統一返回相同的錯誤訊息（避免洩漏帳號是否存在）
        //        return Failure("帳號或密碼錯誤", "Invalid_Credentials", 401);
        //    }
        //}


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

            // 手動登出時，讓 20 分鐘免驗證失效，強制下次登入重新寄 Email 驗證碼
            var userIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (int.TryParse(userIdClaim, out var userId))
            {
                await _loginLogService.InvalidateRecentLoginAsync(userId);
            }

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

        #region 忘記密碼功能

        /// <summary>
        /// 請求密碼重置 - 發送重置連結到郵箱
        /// </summary>
        /// <param name="model">包含用戶郵箱的請求</param>
        /// <returns>成功消息</returns>
        /// <response code="200">重置連結已發送（即使郵箱不存在也返回此消息以防止郵箱枚舉）</response>
        /// <response code="400">請求格式錯誤</response>
        [HttpPost("forgot-password")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [Tags("身分驗證")]
        public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordDTO model)
        {
            Log.Debug("[AuthController] ForgotPassword - Email: {Email}", model.Email);

            if (string.IsNullOrWhiteSpace(model.Email))
            {
                return Failure("Email 不可為空", "Invalid_Input", 400);
            }

            var (success, message) = await _authService.RequestPasswordResetAsync(model.Email);

            // 無論成功或失敗，都返回相同的消息（安全考量）
            return Success(new { Message = "如果該電子郵件存在於我們的系統中，您將收到密碼重置連結" });
        }

        /// <summary>
        /// 驗證重置 Token 是否有效
        /// </summary>
        /// <param name="model">包含郵箱和 Token 的驗證請求</param>
        /// <returns>Token 驗證結果</returns>
        /// <response code="200">Token 有效</response>
        /// <response code="400">Token 無效或已過期</response>
        [HttpPost("verify-reset-token")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [Tags("身分驗證")]
        public async Task<IActionResult> VerifyResetToken([FromBody] VerifyResetTokenDTO model)
        {
            Log.Debug("[AuthController] VerifyResetToken - Email: {Email}", model.Email);

            if (string.IsNullOrWhiteSpace(model.Email) || string.IsNullOrWhiteSpace(model.ResetToken))
            {
                return Failure("Email 和 Token 不可為空", "Invalid_Input", 400);
            }

            var (isValid, message) = await _authService.VerifyResetTokenAsync(model.Email, model.ResetToken);

            if (isValid)
            {
                return Success(new { Message = message, IsValid = true });
            }
            else
            {
                return Failure(message, "Invalid_Token", 400);
            }
        }

        /// <summary>
        /// 重置密碼
        /// </summary>
        /// <param name="model">包含郵箱、Token 和新密碼的重置請求</param>
        /// <returns>密碼重置結果</returns>
        /// <response code="200">密碼重置成功</response>
        /// <response code="400">Token 無效、密碼不符合要求或密碼不匹配</response>
        [HttpPost("reset-password")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [Tags("身分驗證")]
        public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordDTO model)
        {
            Log.Debug("[AuthController] ResetPassword - Email: {Email}", model.Email);

            // 驗證輸入
            if (string.IsNullOrWhiteSpace(model.Email) || 
                string.IsNullOrWhiteSpace(model.ResetToken) ||
                string.IsNullOrWhiteSpace(model.NewPassword))
            {
                return Failure("所有欄位都不可為空", "Invalid_Input", 400);
            }

            // 驗證密碼長度
            if (model.NewPassword.Length < 6)
            {
                return Failure("密碼長度不能少於 6 位", "Password_Too_Short", 400);
            }

            // 驗證密碼匹配
            if (model.NewPassword != model.ConfirmPassword)
            {
                return Failure("兩次輸入的密碼不一致", "Password_Mismatch", 400);
            }

            var (success, message) = await _authService.ResetPasswordAsync(
                model.Email, 
                model.ResetToken, 
                model.NewPassword);

            if (success)
            {
                Log.Information("[AuthController] ResetPassword - 密碼重置成功: {Email}", model.Email);
                return Success(new { Message = message });
            }
            else
            {
                Log.Warning("[AuthController] ResetPassword - 密碼重置失敗: {Email}, 原因: {Message}", model.Email, message);
                return Failure(message, "Reset_Failed", 400);
            }
        }

        #endregion

        /// <summary>
        /// Google OAuth 登入
        /// </summary>
        /// <param name="model">Google Token (支援 ID Token 或 Access Token)</param>
        /// <returns>登入成功返回 Token 和使用者資訊</returns>
        /// <response code="200">登入成功</response>
        /// <response code="401">Token 驗證失敗</response>
        /// <response code="500">伺服器錯誤</response>
        [HttpPost("google-login")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [Tags("身分驗證")]
        public async Task<IActionResult> GoogleLogin([FromBody] GoogleLoginDTO model)
        {
            try
            {
                // 取得客戶端 IP 和 User Agent
                var ipAddress = GetClientIpAddress();
                var userAgent = Request.Headers["User-Agent"].ToString();

                // 取得 Google Token（優先使用 AccessToken，否則使用 IdToken）
                var googleToken = model.GetToken();
                if (string.IsNullOrEmpty(googleToken))
                {
                    Log.Warning("[AuthController] Google 登入請求缺少 Token");
                    return Failure("缺少 Token", "Missing_Token", 400);
                }

                Log.Debug("[AuthController] Google 登入請求，Token 類型: {TokenType}, Token 長度: {Length}",
                    !string.IsNullOrEmpty(model.AccessToken) ? "Access Token" : "ID Token", googleToken.Length);

                // 1. 驗證 Google Token（支援 ID Token 和 Access Token）
                var googleUser = await _googleOAuthService.VerifyGoogleTokenAsync(googleToken);
                if (googleUser == null)
                {
                    // 記錄失敗的登入嘗試
                    await _loginLogService.LogFailedLoginAsync(
                        "unknown",
                        ipAddress,
                        userAgent,
                        "Google Token 驗證失敗");

                    return Failure("Google Token 驗證失敗", "Invalid_Google_Token", 401);
                }

                // 2. 登入或註冊使用者
                var (success, token, userId, isNewUser) = await _googleOAuthService.GoogleLoginOrRegisterAsync(googleUser);

                if (!success || string.IsNullOrEmpty(token))
                {
                    // 記錄失敗的登入嘗試
                    await _loginLogService.LogFailedLoginAsync(
                        googleUser.Email,
                        ipAddress,
                        userAgent,
                        "Google OAuth 登入/註冊失敗");

                    return Failure("登入失敗", "Login_Failed", 500);
                }

                // 3. 設定 Cookie
                Response.Cookies.Append("X-Access-Token", token, new CookieOptions
                {
                    HttpOnly = true,
                    Secure = Request.IsHttps,
                    SameSite = SameSiteMode.Lax,
                    Path = "/",
                    Expires = DateTimeOffset.UtcNow.AddHours(2)
                });

                // 4. 記錄成功的登入
                await _loginLogService.LogUserLoginAsync(
                    userId,
                    googleUser.Email,
                    ipAddress,
                    userAgent);

                // 5. 取得使用者完整資訊
                var userInfo = await _memberProfileService.GetUserInfoByEmailAsync(googleUser.Email);

                Log.Information("[AuthController] Google OAuth 登入成功，Email: {Email}, UserId: {UserId}, IsNewUser: {IsNewUser}",
                    googleUser.Email, userId, isNewUser);

                return Success(new
                {
                    Token = token,
                    User = userInfo,
                    IsNewUser = isNewUser,
                    Message = isNewUser ? "註冊並登入成功" : "登入成功"
                });
            }
            catch (Exception ex)
            {
                Log.Error(ex, "[AuthController] Google OAuth 登入時發生錯誤");
                return Failure("伺服器錯誤", "Server_Error", 500);
            }
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

