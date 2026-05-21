using Microsoft.EntityFrameworkCore;
using PawsPort.Dtos;
using PawsPort.Models;
using System.Text;
using System.Text.Json;

namespace PawsPort.Services
{
    public class LineBotService
    {
        private readonly PetDbContext _db;
        private readonly IConfiguration _config;

        public LineBotService(PetDbContext db, IConfiguration config)
        {
            _db = db;
            _config = config;
        }


        public async Task<(List<LineBotDTO> Items, int TotalPages)> GetPagedMessagesAsync(int page, int pageSize = 10)
        {
            var query = _db.LineBots.AsQueryable();

            query = query.OrderByDescending(b => b.ChatDate);

            int totalItems = await query.CountAsync();
            int totalPages = (int)Math.Ceiling((double)totalItems / pageSize);

            var items = await query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(b => new LineBotDTO
                {
                    Id = b.Id,
                    UserId = b.UserId,
                    ChatDate = b.ChatDate,
                    QuestionType = b.QuestionType,
                    ChiefComplaint = b.ChiefComplaint,
                    ChatContent = b.ChatContent,
                    ReplyContent = b.ReplyContent,
                    ReplyDate = b.ReplyDate,
                    Status = b.Status
                })
                .ToListAsync();

            return (items, totalPages);
        }



        public async Task<bool> ReplyMessageAsync(int id, LineBotReplyDTO dto)
        {

            var message = await _db.LineBots.FindAsync(id);
            if (message == null) return false;


            string channelAccessToken = _config["LineBotToken"];


            // 等未來 Users 表格加了欄位，這段就會改成類似：
            // var targetLineId = await _db.Users.Where(u => u.UserId == message.UserId).Select(u => u.LineId).FirstOrDefaultAsync();

            string targetLineId = "";

            // 假資料
            if (message.UserId == 110)
            {
                targetLineId = _config["TestLineId"];
            }
            else if (message.UserId == 102)
            {
                targetLineId = "U123456nrji3t9tjign3gip3qgnjui3nh";
            }

            // 查不到就直接中斷，不發送
            if (string.IsNullOrEmpty(targetLineId))
            {
                return false;
            }


            // 3. 準備要傳給 LINE 的資料格式 (JSON)
            var requestBody = new
            {
                to = targetLineId,
                messages = new[]
                {
                    new { type = "text", text = dto.ReplyText }
                }
            };

            using (var client = new HttpClient())
            {
                client.DefaultRequestHeaders.Add("Authorization", $"Bearer {channelAccessToken}");
                var content = new StringContent(
                    JsonSerializer.Serialize(requestBody),
                    Encoding.UTF8,
                    "application/json"
                );

                var response = await client.PostAsync("https://api.line.me/v2/bot/message/push", content);

                if (response.IsSuccessStatusCode)
                {
                    message.Status = "已回覆";
                    message.ReplyContent = dto.ReplyText;
                    message.ReplyDate = DateTime.Now;

                    _db.LineBots.Update(message);
                    await _db.SaveChangesAsync();

                    return true;
                }

                return false;
            }
        }


        // 處理 LINE 傳來的客訴訊息並存入資料庫
        public async Task SaveReceivedMessageAsync(string lineUserId, string userRawMessage)
        {
            var newChat = new LineBot
            {
                // 實務上會用 lineUserId 去查對應的會員 ID，這裡先寫死 110 測試
                UserId = 110,
                ChatContent = userRawMessage,
                ChatDate = DateTime.Now,
                QuestionType = "其他", // 預設分類
                Status = "未回覆" // 預設為未回覆
            };

            _db.LineBots.Add(newChat);
            await _db.SaveChangesAsync();
        }
    }
}

