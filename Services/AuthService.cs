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

        public async Task<(bool success, int userId)> RegisterUser(UserRegisterDTO model)
        {
            try
            {
                // 1. 檢查 Email 是否已存在
                var existingUser = await _context.UserAuthTables
                    .AnyAsync(x => x.Email == model.Email);

                if (existingUser)
                {
                    return (false, 0); // Email 已被註冊
                }

                // 2. 先建立 UserTable 實體並儲存以取得 UserId
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
                await _context.SaveChangesAsync(); // 儲存後 userEntity.UserId 會被自動填入

                // 3. 將明文密碼進行 BCrypt 雜湊（自動加鹽）
                string hashedPassword = BCrypt.Net.BCrypt.HashPassword(model.Password);

                // 4. 使用取得的 UserId 建立 UserAuthTable 實體
                var userAuth = new UserAuthTable
                {
                    Email = model.Email,
                    Password = hashedPassword,
                    UserId = userEntity.UserId // 使用新增的 UserId
                };

                _context.UserAuthTables.Add(userAuth);
                await _context.SaveChangesAsync();

                return (true, userEntity.UserId); // 註冊成功，返回 UserId
            }
            catch (Exception)
            {
                return (false, 0); // 註冊失敗
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
    }
}
