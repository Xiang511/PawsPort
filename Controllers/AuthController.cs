using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
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
        public AuthController(AuthService authService)
        {
            _authService = authService;
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
                var token = await _authService.ValidateUser(model.UserEmail,model.Password);

                if (!string.IsNullOrEmpty(token))
                {
                    
                    return Success(new { Token = token });
                }
                else
                {
                    return Failure("帳號或密碼錯誤", "Invalid_Credentials", 401);
                }

            }
            
        }

        
    }
}

