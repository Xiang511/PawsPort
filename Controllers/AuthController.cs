using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json.Linq;
using PawsPort.Dtos;
using PawsPort.Models;
using PawsPort.Services;
using Serilog;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace PawsPort.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Produces("application/json")]
    public class AuthController : ApiControllerBase
    {
        private readonly AuthService _authService;
        private readonly MemberProfileService _memberProfileService;
        public AuthController(AuthService authService,MemberProfileService memberProfileService)
        {
            _authService = authService;
            _memberProfileService = memberProfileService;
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginDTO model)
        {
            if (model.UserEmail == null)
            {
                return Failure("使用者不存在", "User_Not_Found", 404);
            }
            else
            {   
                var userInfo = await _memberProfileService.GetUserInfoByEmailAsync(model.UserEmail);
                var token = await _authService.ValidateUser(model.UserEmail,model.Password);

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
        [HttpPost("logout")]
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

    }
}

