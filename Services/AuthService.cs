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

        public async Task<string> ValidateUser(string email,string password)
        {

            bool isExist = await _context.UserAuthTables
               .AnyAsync(x => x.Email == email && x.Password == password);

            if (isExist)
            {
                // 2. 產生 Token
                var token = GenerateJwtToken(email);
                return token;
            }
            else
            {
                return string.Empty;
            }
        }


        public string GenerateJwtToken(string UserEmail)
        {
            var jwtSettings = _configuration.GetSection("JwtSettings");
            var secretKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings["SecretKey"]));

            // 定義 Claims (聲明)，這些資訊會加密在 Token 中
            var claims = new[]
            {
            new Claim(ClaimTypes.Email, UserEmail),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()) // Token 唯一識別碼
        };

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
