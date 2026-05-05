using Microsoft.AspNetCore.Mvc;
using PawsPort.Dtos;
using PawsPort.Models;
using PawsPort.Services;
using PawsPort.ViewModels;
using Serilog;
namespace PawsPort.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Produces("application/json")]
    public class BlacklistController : ApiControllerBase
    {
        private readonly MemberBlockListService _memberBlockListService;

        public BlacklistController(PetDbContext context, MemberBlockListService memberBlockListService)
        {
            _memberBlockListService = memberBlockListService;
        }
        [HttpGet("users/status")]
        public async Task<IActionResult> BlockList()
        {
            var bannedUsers = await _memberBlockListService.GetBannedUsersAsync();

            return Success(bannedUsers, "Success", 200);

        }

        [HttpPost("user/{id}/status")]
        public async Task<IActionResult> BlockListCreate(int? id , MemberBlockListEditDTO user)
        {
           var result = await _memberBlockListService.CreateBannedUsersAsync(id,user);
            Log.Information("封鎖會員成功 名稱{Name}", result.Name);
            return Success(result, "Success", 200);
        }
    }
}
