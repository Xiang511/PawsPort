using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using NuGet.Common;
using PawsPort.Dtos;
using PawsPort.Models;
using Serilog;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace PawsPort.Services
{
    public class AuthService
    {
        private readonly IConfiguration _configuration;
        private readonly MemberProfileService _memberProfileService;
        private readonly PetDbContext _context;
        private readonly EmailService _emailService;

        public AuthService(IConfiguration configuration, MemberProfileService memberProfileService, PetDbContext PetDbContext, EmailService emailService)
        {
            _configuration = configuration;
            _memberProfileService = memberProfileService;
            _context = PetDbContext;
            _emailService = emailService;
        }

        public async Task<string> ValidateUser(string email, string password)
        {
            // 1. 驗證使用者帳號密碼
            var userId = await _context.UserAuthTables
             .Where(x => x.Email == email)
             .Select(x => new
             {
                 userId = x.UserId
             })
             .FirstOrDefaultAsync();

            var userInfo = await _context.UserAuthTables
            .Where(x => x.Email == email)
            .FirstOrDefaultAsync();

            if (userId == null)
            {
                return string.Empty; // 驗證失敗
            }
            bool isPasswordValid = BCrypt.Net.BCrypt.Verify(password, userInfo.Password);

            if (isPasswordValid)
            {
                // 2. 取得使用者的所有權限 (只需要 SystemId 和 RoleId)
                var userPermissions = await _context.UserSystemRoles
                    .Where(x => x.UserId == userId.userId)
                    .Select(usr => new MemberPermissionUpdateRoleDTO
                    {
                        SystemId = usr.SystemId,
                        RoleId = usr.RoleId
                    })
                    .ToListAsync();

                // 3. 產生包含權限的 Token
                var token = GenerateJwtToken(email, userId.userId, userPermissions);
                return token;
            }
            else
            {
                return string.Empty; // 驗證失敗
            }

        }


        private string GenerateJwtToken(string userEmail, int userId, List<MemberPermissionUpdateRoleDTO> userPermissions)
        {
            var jwtSettings = _configuration.GetSection("JwtSettings");
            var secretKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings["SecretKey"]));

            // 定義基本 Claims
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Email, userEmail),
                new Claim(ClaimTypes.NameIdentifier, userId.ToString()),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
            };

            // 只加入 SystemId 和 RoleId (格式: SystemId:RoleId)
            foreach (var permission in userPermissions)
            {
                claims.Add(new Claim("Permission", $"{permission.SystemId}:{permission.RoleId}"));
            }

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims),
                Expires = DateTime.UtcNow.AddMinutes(double.Parse(jwtSettings["ExpireMinutes"])),
                Issuer = jwtSettings["Issuer"],
                Audience = jwtSettings["Audience"],
                SigningCredentials = new SigningCredentials(secretKey, SecurityAlgorithms.HmacSha256Signature)
            };

            var tokenHandler = new JwtSecurityTokenHandler();
            var token = tokenHandler.CreateToken(tokenDescriptor);

            return tokenHandler.WriteToken(token);
        }

        public async Task<(bool success, string message)> RegisterUser(UserRegisterDTO model)
        {
            try
            {
                // 1. 檢查 Email 是否已被已驗證的帳號使用
                var existingVerified = await _context.UserAuthTables
                    .Join(_context.UserTables,
                        auth => auth.UserId,
                        user => user.UserId,
                        (auth, user) => new { auth, user })
                    .AnyAsync(x => x.auth.Email == model.Email && x.user.IsVerify == true);

                if (existingVerified)
                {
                    return (false, "此 Email 已被註冊");
                }

                // 2. 若此 Email 有殘留的未驗證帳號，先刪除（允許重新註冊）
                var existingUnverified = await _context.UserAuthTables
                    .Join(_context.UserTables,
                        auth => auth.UserId,
                        user => user.UserId,
                        (auth, user) => new { auth, user })
                    .Where(x => x.auth.Email == model.Email && x.user.IsVerify == false)
                    .FirstOrDefaultAsync();

                if (existingUnverified != null)
                {
                    _context.UserAuthTables.Remove(existingUnverified.auth);
                    _context.UserTables.Remove(existingUnverified.user);
                    await _context.SaveChangesAsync();
                    Log.Information("[AuthService] RegisterUser - 已清除舊的未驗證帳號: {Email}", model.Email);
                }

                // 3. 建立 UserTable，IsVerify 設為 false（等待 Email 驗證）
                var userEntity = new UserTable
                {
                    Name = model.Name,
                    Point = 0,
                    IsVerify = false,
                    IsSubscribe = false,
                    CreatedAt = DateTime.Now,
                    UpdatedAt = DateTime.Now,
                    DeleteDay = null,
                    Status = true
                };

                _context.UserTables.Add(userEntity);
                await _context.SaveChangesAsync();

                // 4. 將明文密碼進行 BCrypt 雜湊
                string hashedPassword = BCrypt.Net.BCrypt.HashPassword(model.Password);

                // 5. 生成 Email 驗證碼（6 位數），有效期 10 分鐘
                var verificationCode = GenerateVerificationCode();

                // 6. 建立 UserAuthTable，儲存驗證碼與到期時間
                var userAuth = new UserAuthTable
                {
                    Email = model.Email,
                    Password = hashedPassword,
                    UserId = userEntity.UserId,
                    EmailConfirmationToken = verificationCode,
                    EmailTokenExpiry = DateTime.Now.AddMinutes(10)
                };

                _context.UserAuthTables.Add(userAuth);
                await _context.SaveChangesAsync();

                // 7. 發送 Email 驗證碼郵件
                var emailSent = await _emailService.SendRegisterVerificationCodeAsync(
                    model.Email, verificationCode, model.Name);

                if (!emailSent)
                {
                    Log.Error("[AuthService] RegisterUser - 驗證碼郵件發送失敗: {Email}", model.Email);
                    return (false, "驗證碼郵件發送失敗，請稍後再試");
                }

                Log.Information("[AuthService] RegisterUser - 已建立未驗證帳號並發送驗證碼: {Email}", model.Email);
                return (true, "驗證碼已發送至您的電子郵件，請於 10 分鐘內完成驗證");
            }
            catch (Exception ex)
            {
                Log.Error(ex, "[AuthService] RegisterUser - 註冊失敗: Email={Email}", model.Email);
                return (false, "系統錯誤，請稍後再試");
            }
        }

        /// <summary>
        /// 步驟2：驗證 Email 驗證碼，驗證通過後正式完成帳號建立
        /// </summary>
        /// <param name="email">用戶郵箱</param>
        /// <param name="verificationCode">6 位數驗證碼</param>
        /// <returns>(success, message, userId)</returns>
        public async Task<(bool success, string message, int userId)> VerifyRegisterEmailAsync(string email, string verificationCode)
        {
            try
            {
                // 1. 查詢尚未驗證的帳號
                var userAuth = await _context.UserAuthTables
                    .Where(x => x.Email == email)
                    .FirstOrDefaultAsync();

                if (userAuth == null)
                {
                    return (false, "找不到此 Email 的註冊紀錄，請重新註冊", 0);
                }

                var userEntity = await _context.UserTables
                    .Where(u => u.UserId == userAuth.UserId)
                    .FirstOrDefaultAsync();

                if (userEntity == null)
                {
                    return (false, "帳號資料異常，請重新註冊", 0);
                }

                // 2. 若已驗證，不需重複操作
                if (userEntity.IsVerify == true)
                {
                    return (false, "此 Email 已完成驗證，請直接登入", 0);
                }

                // 3. 檢查驗證碼是否存在
                if (string.IsNullOrEmpty(userAuth.EmailConfirmationToken))
                {
                    return (false, "找不到驗證碼，請重新發送驗證碼", 0);
                }

                // 4. 檢查驗證碼是否過期
                if (userAuth.EmailTokenExpiry == null || userAuth.EmailTokenExpiry < DateTime.Now)
                {
                    Log.Warning("[AuthService] VerifyRegisterEmail - 驗證碼已過期: {Email}", email);
                    // 清除過期的驗證碼
                    userAuth.EmailConfirmationToken = null;
                    userAuth.EmailTokenExpiry = null;
                    await _context.SaveChangesAsync();
                    return (false, "驗證碼已過期，請重新發送驗證碼", 0);
                }

                // 5. 比對驗證碼
                if (userAuth.EmailConfirmationToken != verificationCode)
                {
                    Log.Warning("[AuthService] VerifyRegisterEmail - 驗證碼錯誤: {Email}", email);
                    return (false, "驗證碼錯誤，請重新確認", 0);
                }

                // 6. 驗證通過：更新帳號狀態為已驗證，清除驗證碼
                userEntity.IsVerify = true;
                userEntity.UpdatedAt = DateTime.Now;
                userAuth.EmailConfirmationToken = null;
                userAuth.EmailTokenExpiry = null;

                // 7. 建立 PlayerProfile
                var playerProfile = new PlayerProfile
                {
                    UserId = userEntity.UserId,
                    CurrentPoint = 0,
                    UserName = userEntity.Name
                };

                _context.PlayerProfiles.Add(playerProfile);
                await _context.SaveChangesAsync();

                Log.Information("[AuthService] VerifyRegisterEmail - Email 驗證成功，帳號已啟用: {Email}, UserId={UserId}",
                    email, userEntity.UserId);

                return (true, "Email 驗證成功，帳號已啟用", userEntity.UserId);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "[AuthService] VerifyRegisterEmail - 驗證失敗: {Email}", email);
                return (false, "系統錯誤，請稍後再試", 0);
            }
        }

        /// <summary>
        /// 生成 6 位數驗證碼
        /// </summary>
        private string GenerateVerificationCode()
        {
            using (var rng = RandomNumberGenerator.Create())
            {
                var bytes = new byte[4];
                rng.GetBytes(bytes);
                var randomNumber = BitConverter.ToUInt32(bytes, 0);
                return (randomNumber % 1000000).ToString("D6");
            }
        }

        /// <summary>
        /// 驗證用戶密碼並發送驗證碼
        /// </summary>
        public async Task<(bool success, string message)> SendLoginVerificationCodeAsync(string email, string password)
        {
            try
            {
                // 1. 驗證用戶帳號密碼
                var userAuth = await _context.UserAuthTables
                    .Where(x => x.Email == email)
                    .FirstOrDefaultAsync();

                if (userAuth == null)
                {
                    Log.Warning("[AuthService] SendLoginVerificationCode - 用戶不存在: {Email}", email);
                    return (false, "帳號或密碼錯誤");
                }

                // 2. 驗證密碼
                bool isPasswordValid = BCrypt.Net.BCrypt.Verify(password, userAuth.Password);
                if (!isPasswordValid)
                {
                    Log.Warning("[AuthService] SendLoginVerificationCode - 密碼錯誤: {Email}", email);
                    return (false, "帳號或密碼錯誤");
                }

                // 3. 生成驗證碼
                var verificationCode = GenerateVerificationCode();
                var expiryTime = DateTime.Now.AddMinutes(5); // 5 分鐘有效期

                // 4. 保存驗證碼到資料庫
                userAuth.EmailConfirmationToken = verificationCode;
                userAuth.EmailTokenExpiry = expiryTime;
                await _context.SaveChangesAsync();

                // 5. 獲取用戶名
                var user = await _context.UserTables
                    .Where(u => u.UserId == userAuth.UserId)
                    .FirstOrDefaultAsync();

                var userName = user?.Name ?? "用戶";

                // 6. 發送驗證碼郵件
                var emailSent = await _emailService.SendVerificationCodeAsync(email, verificationCode, userName);

                if (!emailSent)
                {
                    Log.Error("[AuthService] SendLoginVerificationCode - 郵件發送失敗: {Email}", email);
                    return (false, "驗證碼發送失敗，請稍後再試");
                }

                Log.Information("[AuthService] SendLoginVerificationCode - 驗證碼已發送: {Email}", email);
                return (true, "驗證碼已發送至您的電子郵件");
            }
            catch (Exception ex)
            {
                Log.Error(ex, "[AuthService] SendLoginVerificationCode - 發送驗證碼失敗: {Email}", email);
                return (false, "系統錯誤，請稍後再試");
            }
        }

        /// <summary>
        /// 驗證驗證碼並完成登入
        /// </summary>
        public async Task<(bool success, string token, string message)> VerifyLoginCodeAsync(string email, string verificationCode)
        {
            try
            {
                // 1. 查找用戶
                var userAuth = await _context.UserAuthTables
                    .Where(x => x.Email == email)
                    .FirstOrDefaultAsync();

                if (userAuth == null)
                {
                    Log.Warning("[AuthService] VerifyLoginCode - 用戶不存在: {Email}", email);
                    return (false, string.Empty, "驗證失敗");
                }

                // 2. 檢查驗證碼是否存在
                if (string.IsNullOrEmpty(userAuth.EmailConfirmationToken))
                {
                    Log.Warning("[AuthService] VerifyLoginCode - 驗證碼不存在: {Email}", email);
                    return (false, string.Empty, "請先獲取驗證碼");
                }

                // 3. 檢查驗證碼是否過期
                if (userAuth.EmailTokenExpiry == null || userAuth.EmailTokenExpiry < DateTime.Now)
                {
                    Log.Warning("[AuthService] VerifyLoginCode - 驗證碼已過期: {Email}", email);
                    // 清除過期的驗證碼
                    userAuth.EmailConfirmationToken = null;
                    userAuth.EmailTokenExpiry = null;
                    await _context.SaveChangesAsync();
                    return (false, string.Empty, "驗證碼已過期，請重新獲取");
                }

                // 4. 驗證碼匹配
                if (userAuth.EmailConfirmationToken != verificationCode)
                {
                    Log.Warning("[AuthService] VerifyLoginCode - 驗證碼錯誤: {Email}", email);
                    return (false, string.Empty, "驗證碼錯誤");
                }

                // 5. 驗證成功，清除驗證碼
                userAuth.EmailConfirmationToken = null;
                userAuth.EmailTokenExpiry = null;
                await _context.SaveChangesAsync();

                // 6. 獲取用戶權限
                var userPermissions = await _context.UserSystemRoles
                    .Where(x => x.UserId == userAuth.UserId)
                    .Select(usr => new MemberPermissionUpdateRoleDTO
                    {
                        SystemId = usr.SystemId,
                        RoleId = usr.RoleId
                    })
                    .ToListAsync();

                // 7. 生成 JWT Token
                var token = GenerateJwtToken(email, userAuth.UserId, userPermissions);

                Log.Information("[AuthService] VerifyLoginCode - 驗證成功: {Email}", email);
                return (true, token, "登入成功");
            }
            catch (Exception ex)
            {
                Log.Error(ex, "[AuthService] VerifyLoginCode - 驗證失敗: {Email}", email);
                return (false, string.Empty, "系統錯誤，請稍後再試");
            }
        }

        /// <summary>
        /// 清除過期的驗證碼（可以用定時任務調用）
        /// </summary>
        public async Task<int> ClearExpiredVerificationCodesAsync()
        {
            try
            {
                var expiredRecords = await _context.UserAuthTables
                    .Where(x => x.EmailTokenExpiry != null && x.EmailTokenExpiry < DateTime.Now)
                    .ToListAsync();

                foreach (var record in expiredRecords)
                {
                    record.EmailConfirmationToken = null;
                    record.EmailTokenExpiry = null;
                }

                await _context.SaveChangesAsync();
                Log.Information("[AuthService] ClearExpiredVerificationCodes - 已清除 {Count} 個過期驗證碼", expiredRecords.Count);
                return expiredRecords.Count;
            }
            catch (Exception ex)
            {
                Log.Error(ex, "[AuthService] ClearExpiredVerificationCodes - 清除失敗");
                return 0;
            }
        }

        #region 忘記密碼功能

        /// <summary>
        /// 生成安全的重置 Token（使用 GUID）
        /// </summary>
        private string GenerateResetToken()
        {
            return Guid.NewGuid().ToString("N"); // 32 位十六進制字符串
        }

        /// <summary>
        /// 請求密碼重置 - 發送重置連結到郵箱
        /// </summary>
        /// <param name="email">用戶郵箱</param>
        /// <returns>(success, message)</returns>
        public async Task<(bool success, string message)> RequestPasswordResetAsync(string email)
        {
            try
            {
                // 1. 查找用戶
                var userAuth = await _context.UserAuthTables
                    .Where(x => x.Email == email)
                    .FirstOrDefaultAsync();

                // 安全考量：無論用戶是否存在，都返回相同的成功消息（防止郵箱枚舉）
                if (userAuth == null)
                {
                    Log.Warning("[AuthService] RequestPasswordReset - 用戶不存在: {Email}", email);
                    // 故意返回成功，不洩露用戶是否存在
                    await Task.Delay(Random.Shared.Next(500, 1500)); // 模擬處理時間
                    return (true, "如果該電子郵件存在於我們的系統中，您將收到密碼重置連結");
                }

                // 2. 生成重置 Token
                var resetToken = GenerateResetToken();
                var expiryTime = DateTime.Now.AddHours(24); // 24 小時有效期

                // 3. 保存到數據庫
                userAuth.PasswordResetToken = resetToken;
                userAuth.PasswordResetExpires = expiryTime;
                await _context.SaveChangesAsync();

                // 4. 獲取用戶名
                var user = await _context.UserTables
                    .Where(u => u.UserId == userAuth.UserId)
                    .FirstOrDefaultAsync();

                var userName = user?.Name ?? "用戶";

                // 5. 發送重置郵件
                var emailSent = await _emailService.SendPasswordResetEmailAsync(email, resetToken, userName);

                if (!emailSent)
                {
                    Log.Error("[AuthService] RequestPasswordReset - 郵件發送失敗: {Email}", email);
                    return (false, "郵件發送失敗，請稍後再試");
                }

                Log.Information("[AuthService] RequestPasswordReset - 重置連結已發送: {Email}", email);
                return (true, "密碼重置連結已發送至您的電子郵件");
            }
            catch (Exception ex)
            {
                Log.Error(ex, "[AuthService] RequestPasswordReset - 請求失敗: {Email}", email);
                return (false, "系統錯誤，請稍後再試");
            }
        }

        /// <summary>
        /// 驗證重置 Token 是否有效
        /// </summary>
        /// <param name="email">用戶郵箱</param>
        /// <param name="resetToken">重置 Token</param>
        /// <returns>(isValid, message)</returns>
        public async Task<(bool isValid, string message)> VerifyResetTokenAsync(string email, string resetToken)
        {
            try
            {
                var userAuth = await _context.UserAuthTables
                    .Where(x => x.Email == email)
                    .FirstOrDefaultAsync();

                if (userAuth == null)
                {
                    Log.Warning("[AuthService] VerifyResetToken - 用戶不存在: {Email}", email);
                    return (false, "無效的重置請求");
                }

                // 檢查 Token 是否存在
                if (string.IsNullOrEmpty(userAuth.PasswordResetToken))
                {
                    Log.Warning("[AuthService] VerifyResetToken - Token 不存在: {Email}", email);
                    return (false, "無效的重置 Token");
                }

                // 檢查 Token 是否匹配
                if (userAuth.PasswordResetToken != resetToken)
                {
                    Log.Warning("[AuthService] VerifyResetToken - Token 不匹配: {Email}", email);
                    return (false, "無效的重置 Token");
                }

                // 檢查是否過期
                if (userAuth.PasswordResetExpires == null || userAuth.PasswordResetExpires < DateTime.Now)
                {
                    Log.Warning("[AuthService] VerifyResetToken - Token 已過期: {Email}", email);
                    // 清除過期的 Token
                    userAuth.PasswordResetToken = null;
                    userAuth.PasswordResetExpires = null;
                    await _context.SaveChangesAsync();
                    return (false, "重置連結已過期，請重新申請");
                }

                Log.Information("[AuthService] VerifyResetToken - Token 有效: {Email}", email);
                return (true, "Token 驗證成功");
            }
            catch (Exception ex)
            {
                Log.Error(ex, "[AuthService] VerifyResetToken - 驗證失敗: {Email}", email);
                return (false, "系統錯誤，請稍後再試");
            }
        }

        /// <summary>
        /// 重置密碼
        /// </summary>
        /// <param name="email">用戶郵箱</param>
        /// <param name="resetToken">重置 Token</param>
        /// <param name="newPassword">新密碼</param>
        /// <returns>(success, message)</returns>
        public async Task<(bool success, string message)> ResetPasswordAsync(string email, string resetToken, string newPassword)
        {
            try
            {
                // 1. 先驗證 Token
                var (isValid, validationMessage) = await VerifyResetTokenAsync(email, resetToken);
                if (!isValid)
                {
                    return (false, validationMessage);
                }

                // 2. 查找用戶
                var userAuth = await _context.UserAuthTables
                    .Where(x => x.Email == email)
                    .FirstOrDefaultAsync();

                if (userAuth == null)
                {
                    Log.Warning("[AuthService] ResetPassword - 用戶不存在: {Email}", email);
                    return (false, "無效的重置請求");
                }

                // 3. 雜湊新密碼
                string hashedPassword = BCrypt.Net.BCrypt.HashPassword(newPassword);

                // 4. 更新密碼並清除重置 Token
                userAuth.Password = hashedPassword;
                userAuth.PasswordResetToken = null;
                userAuth.PasswordResetExpires = null;
                await _context.SaveChangesAsync();

                Log.Information("[AuthService] ResetPassword - 密碼已重置: {Email}", email);
                return (true, "密碼重置成功，請使用新密碼登入");
            }
            catch (Exception ex)
            {
                Log.Error(ex, "[AuthService] ResetPassword - 重置失敗: {Email}", email);
                return (false, "系統錯誤，請稍後再試");
            }
        }

        /// <summary>
        /// 清除過期的重置 Token（可以用定時任務調用）
        /// </summary>
        public async Task<int> ClearExpiredResetTokensAsync()
        {
            try
            {
                var expiredRecords = await _context.UserAuthTables
                    .Where(x => x.PasswordResetExpires != null && x.PasswordResetExpires < DateTime.Now)
                    .ToListAsync();

                foreach (var record in expiredRecords)
                {
                    record.PasswordResetToken = null;
                    record.PasswordResetExpires = null;
                }

                await _context.SaveChangesAsync();
                Log.Information("[AuthService] ClearExpiredResetTokens - 已清除 {Count} 個過期重置 Token", expiredRecords.Count);
                return expiredRecords.Count;
            }
            catch (Exception ex)
            {
                Log.Error(ex, "[AuthService] ClearExpiredResetTokens - 清除失敗");
                return 0;
            }
        }

        #endregion
    }
}
