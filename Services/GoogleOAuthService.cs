using Microsoft.EntityFrameworkCore;
using PawsPort.Dtos;
using PawsPort.Models;
using Serilog;
using System.Text.Json;

namespace PawsPort.Services
{
    /// <summary>
    /// Google OAuth 服務
    /// </summary>
    public class GoogleOAuthService
    {
        private readonly PetDbContext _context;
        private readonly IConfiguration _configuration;
        private readonly HttpClient _httpClient;
        private readonly AuthService _authService;
        private readonly MemberPermissionService _memberPermissionService;

        public GoogleOAuthService(
            PetDbContext context,
            IConfiguration configuration,
            IHttpClientFactory httpClientFactory,
            AuthService authService,
            MemberPermissionService memberPermissionService)
        {
            _context = context;
            _configuration = configuration;
            _httpClient = httpClientFactory.CreateClient();
            _authService = authService;
            _memberPermissionService = memberPermissionService;
        }

        /// <summary>
        /// 驗證 Google Token 並取得使用者資訊
        /// 支援兩種方式：
        /// 1. ID Token (credential) - 使用 tokeninfo API
        /// 2. Access Token - 使用 userinfo API
        /// </summary>
        public async Task<GoogleUserInfoDTO?> VerifyGoogleTokenAsync(string token)
        {
            try
            {
                var clientId = _configuration["GoogleOAuth:ClientId"];
                if (string.IsNullOrEmpty(clientId))
                {
                    Log.Error("[GoogleOAuthService] Google OAuth ClientId 未設定");
                    return null;
                }

                Log.Debug("[GoogleOAuthService] 開始驗證 Google Token，ClientId: {ClientId}", clientId);
                Log.Debug("[GoogleOAuthService] Token 長度: {Length}", token?.Length);

                // 先嘗試方法 1：使用 Access Token 呼叫 userinfo API (推薦)
                try
                {
                    Log.Debug("[GoogleOAuthService] 嘗試使用 Access Token 呼叫 userinfo API");
                    var userinfoUrl = "https://www.googleapis.com/oauth2/v3/userinfo";

                    var request = new HttpRequestMessage(HttpMethod.Get, userinfoUrl);
                    request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

                    var userinfoResponse = await _httpClient.SendAsync(request);

                    Log.Debug("[GoogleOAuthService] Userinfo API 回應狀態: {StatusCode}", userinfoResponse.StatusCode);

                    if (userinfoResponse.IsSuccessStatusCode)
                    {
                        var content = await userinfoResponse.Content.ReadAsStringAsync();
                        Log.Debug("[GoogleOAuthService] Userinfo API 回應內容: {Content}", content);

                        var googleUser = JsonSerializer.Deserialize<GoogleUserInfoDTO>(content, new JsonSerializerOptions
                        {
                            PropertyNameCaseInsensitive = true
                        });

                        if (googleUser != null && !string.IsNullOrEmpty(googleUser.Email))
                        {
                            Log.Information("[GoogleOAuthService] Access Token 驗證成功，Email: {Email}, Name: {Name}", 
                                googleUser.Email, googleUser.Name);
                            return googleUser;
                        }
                    }
                }
                catch (Exception ex)
                {
                    Log.Debug(ex, "[GoogleOAuthService] Access Token 驗證失敗，嘗試 ID Token 驗證");
                }

                // 方法 2：使用 ID Token 呼叫 tokeninfo API (備用)
                Log.Debug("[GoogleOAuthService] 嘗試使用 ID Token 呼叫 tokeninfo API");
                var tokeninfoUrl = $"https://oauth2.googleapis.com/tokeninfo?id_token={token}";
                var tokeninfoResponse = await _httpClient.GetAsync(tokeninfoUrl);

                Log.Debug("[GoogleOAuthService] Tokeninfo API 回應狀態: {StatusCode}", tokeninfoResponse.StatusCode);

                if (!tokeninfoResponse.IsSuccessStatusCode)
                {
                    var errorContent = await tokeninfoResponse.Content.ReadAsStringAsync();
                    Log.Warning("[GoogleOAuthService] Google Token 驗證失敗，StatusCode: {StatusCode}, Error: {Error}", 
                        tokeninfoResponse.StatusCode, errorContent);
                    return null;
                }

                var tokeninfoContent = await tokeninfoResponse.Content.ReadAsStringAsync();
                Log.Debug("[GoogleOAuthService] Tokeninfo API 回應內容: {Content}", tokeninfoContent);

                var googleUserFromToken = JsonSerializer.Deserialize<GoogleUserInfoDTO>(tokeninfoContent, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });

                if (googleUserFromToken == null)
                {
                    Log.Error("[GoogleOAuthService] 無法解析 Google 使用者資訊");
                    return null;
                }

                // 驗證 audience (aud) 是否符合
                var jsonDoc = JsonDocument.Parse(tokeninfoContent);
                if (jsonDoc.RootElement.TryGetProperty("aud", out var audElement))
                {
                    var aud = audElement.GetString();
                    Log.Debug("[GoogleOAuthService] Token Audience: {Aud}, Expected: {ClientId}", aud, clientId);

                    if (aud != clientId)
                    {
                        Log.Warning("[GoogleOAuthService] Token audience 不符，Expected: {ClientId}, Got: {Aud}", clientId, aud);
                        return null;
                    }
                }

                Log.Information("[GoogleOAuthService] ID Token 驗證成功，Email: {Email}, Name: {Name}", 
                    googleUserFromToken.Email, googleUserFromToken.Name);
                return googleUserFromToken;
            }
            catch (Exception ex)
            {
                Log.Error(ex, "[GoogleOAuthService] 驗證 Google Token 時發生錯誤");
                return null;
            }
        }

        /// <summary>
        /// Google OAuth 登入或註冊
        /// </summary>
        public async Task<(bool success, string? token, int userId, bool isNewUser)> GoogleLoginOrRegisterAsync(GoogleUserInfoDTO googleUser)
        {
            try
            {
                // 1. 先檢查是否已有 OAuth 記錄（使用 Google 的 sub 作為 ProviderKey）
                var existingOAuth = await _context.OauthTables
                    .FirstOrDefaultAsync(x => x.AuthType == "Google" && x.ProviderKey == googleUser.Sub);

                int userId;
                bool isNewUser = false;

                if (existingOAuth != null)
                {
                    // 已存在的 Google 使用者 - 直接登入
                    userId = existingOAuth.UserId;
                    Log.Information("[GoogleOAuthService] 現有 Google 使用者登入，UserId: {UserId}, Email: {Email}", 
                        userId, googleUser.Email);
                }
                else
                {
                    // 檢查該 Email 是否已經在系統中（可能是一般註冊的使用者）
                    var existingUser = await _context.UserAuthTables
                        .FirstOrDefaultAsync(x => x.Email == googleUser.Email);

                    if (existingUser != null)
                    {
                        // Email 已存在（一般註冊使用者），將 Google OAuth 綁定到現有帳號
                        userId = existingUser.UserId;
                        Log.Information("[GoogleOAuthService] 將 Google OAuth 綁定到現有帳號，UserId: {UserId}, Email: {Email}", 
                            userId, googleUser.Email);

                        // 建立 OAuth 記錄
                        var oauthRecord = new OauthTable
                        {
                            AuthType = "Google",
                            ProviderKey = googleUser.Sub, // Google 的唯一識別碼
                            AccessToken = null, // Access Token 可選
                            RefreshToken = null, // Refresh Token 可選
                            UserId = userId
                        };

                        _context.OauthTables.Add(oauthRecord);
                        await _context.SaveChangesAsync();
                    }
                    else
                    {
                        // 全新使用者 - 建立帳號
                        Log.Information("[GoogleOAuthService] 新 Google 使用者註冊，Email: {Email}", googleUser.Email);
                        isNewUser = true;

                        // 建立 UserTable
                        var userEntity = new UserTable
                        {
                            Name = googleUser.Name ?? googleUser.Email.Split('@')[0],
                            Point = 0,
                            IsVerify = googleUser.EmailVerified, // Google 已驗證 Email
                            IsSubscribe = false,
                            CreatedAt = DateTime.Now,
                            UpdatedAt = DateTime.Now,
                            DeleteDay = null,
                            Status = true
                        };

                        _context.UserTables.Add(userEntity);
                        await _context.SaveChangesAsync();

                        userId = userEntity.UserId;

                        // 建立 UserAuthTable（使用隨機密碼，因為不允許 NULL）
                        var randomPassword = Guid.NewGuid().ToString(); // 隨機密碼（使用者無法使用此密碼登入）
                        var userAuth = new UserAuthTable
                        {
                            Email = googleUser.Email,
                            Password = BCrypt.Net.BCrypt.HashPassword(randomPassword), // 雜湊隨機密碼
                            UserId = userId
                        };

                        _context.UserAuthTables.Add(userAuth);
                        await _context.SaveChangesAsync();

                        // 7. 建立 PlayerProfile
                        var playerProfile = new PlayerProfile
                        {
                            UserId = userEntity.UserId,
                            CurrentPoint = 0,
                            UserName = userEntity.Name
                        };

                        _context.PlayerProfiles.Add(playerProfile);
                        await _context.SaveChangesAsync();

                        var inventory = new Inventory
                        {
                            PlayerId = playerProfile.PlayerId,
                            SkinId = 2, // 預設道具 ID
                            Enable = true
                        };
                        _context.Inventories.Add(inventory);
                        await _context.SaveChangesAsync();
                         

                        // 建立 OAuth 記錄
                        var oauthRecord = new OauthTable
                        {
                            AuthType = "Google",
                            ProviderKey = googleUser.Sub, // Google 的唯一識別碼
                            AccessToken = null,
                            RefreshToken = null,
                            UserId = userId
                        };

                        _context.OauthTables.Add(oauthRecord);
                        await _context.SaveChangesAsync();

                        // 為新用戶分配預設權限（除會員系統外的所有系統一般成員權限）
                        var systemsToAssign = new[] { 2, 3, 4, 5 }; // 寵物、遊戲、客服、社群
                        var generalMemberRoleId = 3; // 一般成員

                        foreach (var systemId in systemsToAssign)
                        {
                            var permissionDto = new MemberUserSystemRoleDTO
                            {
                                UserId = userId,
                                SystemId = systemId,
                                RoleId = generalMemberRoleId
                            };

                            await _memberPermissionService.CreateMemberPermissionAsync(permissionDto);
                        }

                        Log.Information("[GoogleOAuthService] Google 新使用者建立成功，UserId: {UserId}", userId);
                    }
                }

                // 4. 取得使用者權限
                var userPermissions = await _context.UserSystemRoles
                    .Where(x => x.UserId == userId)
                    .Select(usr => new MemberPermissionUpdateRoleDTO
                    {
                        SystemId = usr.SystemId,
                        RoleId = usr.RoleId
                    })
                    .ToListAsync();

                // 5. 產生 JWT Token
                var token = GenerateJwtToken(googleUser.Email, userId, userPermissions);

                return (true, token, userId, isNewUser);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "[GoogleOAuthService] Google OAuth 登入/註冊失敗");
                return (false, null, 0, false);
            }
        }

        /// <summary>
        /// 產生 JWT Token（與 AuthService 相同邏輯）
        /// </summary>
        private string GenerateJwtToken(string email, int userId, List<MemberPermissionUpdateRoleDTO> userPermissions)
        {
            var jwtSettings = _configuration.GetSection("JwtSettings");
            var secretKey = new Microsoft.IdentityModel.Tokens.SymmetricSecurityKey(
                System.Text.Encoding.UTF8.GetBytes(jwtSettings["SecretKey"]));

            var claims = new List<System.Security.Claims.Claim>
            {
                new System.Security.Claims.Claim(System.Security.Claims.ClaimTypes.Email, email),
                new System.Security.Claims.Claim(System.Security.Claims.ClaimTypes.NameIdentifier, userId.ToString()),
                new System.Security.Claims.Claim(System.IdentityModel.Tokens.Jwt.JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
            };

            // 加入權限 Claims
            foreach (var permission in userPermissions)
            {
                claims.Add(new System.Security.Claims.Claim("Permission", $"{permission.SystemId}:{permission.RoleId}"));
            }

            var tokenDescriptor = new Microsoft.IdentityModel.Tokens.SecurityTokenDescriptor
            {
                Subject = new System.Security.Claims.ClaimsIdentity(claims),
                Expires = DateTime.UtcNow.AddMinutes(double.Parse(jwtSettings["ExpireMinutes"])),
                Issuer = jwtSettings["Issuer"],
                Audience = jwtSettings["Audience"],
                SigningCredentials = new Microsoft.IdentityModel.Tokens.SigningCredentials(
                    secretKey, Microsoft.IdentityModel.Tokens.SecurityAlgorithms.HmacSha256Signature)
            };

            var tokenHandler = new System.IdentityModel.Tokens.Jwt.JwtSecurityTokenHandler();
            var token = tokenHandler.CreateToken(tokenDescriptor);

            return tokenHandler.WriteToken(token);
        }
    }
}
