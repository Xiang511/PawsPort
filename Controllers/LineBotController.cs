using Microsoft.AspNetCore.Mvc;
using PawsPort.Dtos;
using PawsPort.Services;
using System.Text.Json;

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
        [Tags("客服管理")]
        public async Task<IActionResult> GetList([FromQuery] int page = 1)
        {
            var (items, totalPages) = await _lineBotService.GetPagedMessagesAsync(page, 10);

            var responseData = new
            {
                items = items,
                totalPages = totalPages,
                currentPage = page
            };

            return Success(responseData, "取得 LineBot 訊息列表成功", 200);
        }


        [HttpPost("{id}/reply")]
        [Tags("客服管理")]

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


            return Success<object>(null, "成功！回覆訊息已透過 LINE Bot 傳送給該名使用者。", 200);
        }


        [HttpPost("webhook")]
        [Tags("客服管理")]
        public async Task<IActionResult> ReceiveMessage([FromBody] JsonElement body)
        {
            try
            {
                if (body.TryGetProperty("events", out var events) && events.GetArrayLength() > 0)
                {
                    var firstEvent = events[0];

                    // 確保這是一個文字訊息事件，而不是貼圖或加好友事件
                    if (firstEvent.TryGetProperty("type", out var type) && type.GetString() == "message")
                    {
                        var messageObj = firstEvent.GetProperty("message");
                        if (messageObj.TryGetProperty("type", out var msgType) && msgType.GetString() == "text")
                        {
                            string lineUserId = firstEvent.GetProperty("source").GetProperty("userId").GetString();
                            string userRawMessage = messageObj.GetProperty("text").GetString();

                            await _lineBotService.SaveReceivedMessageAsync(lineUserId, userRawMessage);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"解析 LINE 訊息失敗: {ex.Message}");
            }

            return Ok();
        }
    }
}

