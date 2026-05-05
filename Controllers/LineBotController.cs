using Microsoft.AspNetCore.Mvc;
using PawsPort.Dtos;
using PawsPort.Services;

namespace PawsPort.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Produces("application/json")]


    public class LineBotController : ApiControllerBase
    {
        private readonly LineBotService _lineBotService;

        public LineBotController(LineBotService lineBotService)
        {
            _lineBotService = lineBotService;
        }


        [HttpGet]
        public async Task<IActionResult> GetList()
        {
            var result = await _lineBotService.GetAllMessagesAsync();
            return Success(result, "取得 LineBot 訊息列表成功", 200);
        }


        [HttpPost("{id}/reply")]
        public async Task<IActionResult> Reply(int id, LineBotReplyDTO dto)
        {
            if (!ModelState.IsValid)
            {
                return Failure("VALIDATION_ERROR", "請填寫回覆內容", 400);
            }

            var isSuccess = await _lineBotService.ReplyMessageAsync(id, dto);

            if (!isSuccess)
            {
                return Failure("MESSAGE_NOT_FOUND", "找不到該筆訊息，無法回覆", 404);
            }


            return Success<object>(null, "✅ 成功！回覆訊息已透過 LINE Bot 傳送給該名使用者。", 200);
        }
    }
}

