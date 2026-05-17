using Microsoft.IdentityModel.Tokens;
using PawsPort.Models;
using Microsoft.EntityFrameworkCore;
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
            var user = await _context.UserAuthTables
             .Where(x => x.Email == email && x.Password == password)
             .Select(x => new
             {
                 userId = x.UserId
             })
             .FirstOrDefaultAsync();

            if (user == null)
            {
                return string.Empty; // 驗證失敗
            }

            // 2. 取得使用者的所有權限 (只需要 SystemId 和 RoleId)
            var userPermissions = await _context.UserSystemRoles
                .Where(x => x.UserId == user.userId)
                .Select(usr => new PermissionDto
                {
                    SystemId = usr.SystemId,
                    RoleId = usr.RoleId
                })
                .ToListAsync();

            // 3. 產生包含權限的 Token
            var token = GenerateJwtToken(email, user.userId, userPermissions);
            return token;
        }

        private class PermissionDto
        {
            public int SystemId { get; set; }
            public int RoleId { get; set; }
        }

        private string GenerateJwtToken(string userEmail, int userId, List<PermissionDto> userPermissions)
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
    }
}
