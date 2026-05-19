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


            string channelAccessToken = "mfFnFw0or3XIbqvxdYwEQ6Miebdv2RWGhBy6QiBiJqGazJaKEUWjMRcS4Puewqs3TGiiggUZe65wNQ0YoqUH9Vw4A85oxRds1JBnjpndxBqO2L+ZiTWgrGQ06yVElV5nF/jFlXK6T6cVSJjdFPXVeAdB04t89/1O/w1cDnyilFU=";


            // 等未來 Users 表格加了欄位，這段就會改成類似：
            // var targetLineId = await _db.Users.Where(u => u.UserId == message.UserId).Select(u => u.LineId).FirstOrDefaultAsync();

            string targetLineId = "";

            // 假資料
            if (message.UserId == 110)
            {
                targetLineId = "U41291dd10ae56ea72207be445b446da3"; //此Id為真
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


                if (!response.IsSuccessStatusCode)
                {

                    return false;
                }
            }

            return true;
        }
    }
}

