using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using NuGet.Common;
using PawsPort.Dtos;
using PawsPort.Models;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace PawsPort.Services
{
    public class AuthService
    {
        private readonly IConfiguration _configuration;
        private readonly MemberProfileService _memberProfileService;
        private readonly PetDbContext _context;

        public AuthService(IConfiguration configuration, MemberProfileService memberProfileService, PetDbContext PetDbContext)
        {
            _configuration = configuration;
            _memberProfileService = memberProfileService;
            _context = PetDbContext;
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

        public async Task<bool> RegisterUser(UserRegisterDTO model)
        {
            try
            {
                // 1. 檢查 Email 是否已存在
                var existingUser = await _context.UserAuthTables
                    .AnyAsync(x => x.Email == model.Email);

                if (existingUser)
                {
                    return false; // Email 已被註冊
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

                return true; // 註冊成功
            }
            catch (Exception)
            {
                return false; // 註冊失敗
            }
        }
    }
}
