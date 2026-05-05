using Microsoft.EntityFrameworkCore;
using PawsPort.Dtos;
using PawsPort.Models;

namespace PawsPort.Services
{
    public class LineBotService
    {
        private readonly PetDbContext _db;

        public LineBotService(PetDbContext db)
        {
            _db = db;
        }


        public async Task<List<LineBotDTO>> GetAllMessagesAsync()
        {
            var typeOrder = new List<string>
            {
                "認養", "醫療", "帳號", "系統", "遊戲", "其他"
            };

            var listFromDb = await _db.LineBots.ToListAsync();

            return listFromDb
                .OrderBy(b =>
                {
                    int index = typeOrder.IndexOf(b.QuestionType);
                    return index == -1 ? 99 : index;
                })
                .ThenByDescending(b => b.ChatDate)
                .Select(b => new LineBotDTO
                {
                    Id = b.Id,
                    UserId = b.UserId,
                    ChatDate = b.ChatDate,
                    QuestionType = b.QuestionType,
                    ChiefComplaint = b.ChiefComplaint,
                    ChatContent = b.ChatContent
                })
                .ToList();
        }


        public async Task<bool> ReplyMessageAsync(int id, LineBotReplyDTO dto)
        {
            var message = await _db.LineBots.FindAsync(id);
            if (message == null) return false;

            // 【未來擴充區】
            // 未來要串接 LINE API 時，可以用 message.UserId 去推播給特定使用者
            // LineApi.PushMessage(message.UserId.ToString(), dto.ReplyText);
            // 等你們資料庫如果加了 ReplyContent 跟 ReplyDate 欄位，也可以在這裡存檔：
            // message.ReplyContent = dto.ReplyText;
            // message.ReplyDate = DateTime.Now;
            // await _db.SaveChangesAsync();


            // 目前先直接回傳 true 代表模擬傳送成功
            return true;
        }
    }
}
