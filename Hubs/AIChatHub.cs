using Microsoft.AspNetCore.SignalR;
using System.Text;
using System.Text.Json;

namespace PawsPort.Hubs
{
    public class AIChatHub : Hub
    {
        private readonly IConfiguration _config;

        // 🟢 依賴注入：把設定檔管理員請進來
        public AIChatHub(IConfiguration config)
        {
            _config = config;
        }

        public async Task SendMessage(string userMessage)
        {
            Console.WriteLine($"[SignalR] 收到前端訊息: {userMessage}");
            await Clients.Caller.SendAsync("ReceiveMessage", "User", userMessage);

            Console.WriteLine("[SignalR] 準備呼叫 Gemini API...");
            string aiReply = await CallGeminiApiAsync(userMessage);

            Console.WriteLine($"[SignalR] 拿到 AI 回覆: {aiReply}");
            await Clients.Caller.SendAsync("ReceiveMessage", "AI", aiReply);
        }

        // 專門負責跟 Google 溝通
        private async Task<string> CallGeminiApiAsync(string prompt)
        {
            // 從 secrets.json 拿出你的鑰匙
            string apiKey = _config["GeminiApiKey"];
            if (string.IsNullOrEmpty(apiKey)) return "系統錯誤：找不到 AI API Key";

            // Gemini 2.5 Flash 模型的官方端點
            string url = $"https://generativelanguage.googleapis.com/v1beta/models/gemini-3.5-flash:generateContent?key={apiKey}";

            // 依照 Google 規定的 JSON 格式打包問題
            var requestBody = new
            {
                // 給 AI 的最高指導原則 (使用者看不到)
                systemInstruction = new
                {
                    parts = new[]
                    {
                        new { text = "你現在是 Petmily 寵物媒合平台的專屬 AI 客服小幫手。你的語氣要親切、充滿熱情，可以適當加上汪汪或喵喵的發語詞。如果使用者問了跟寵物或本平台無關的問題，請委婉地將話題引導回寵物上。回答請盡量簡潔，控制在 50 個字以內。" }
                    }
                },
                // 這是使用者實際輸入的問題
                contents = new[]
                {
                    new { parts = new[] { new { text = prompt } } }
                }
            };

            using var client = new HttpClient();
            var content = new StringContent(JsonSerializer.Serialize(requestBody), Encoding.UTF8, "application/json");

            try
            {
                // 發送請求給 Google
                var response = await client.PostAsync(url, content);

                if (response.IsSuccessStatusCode)
                {
                    // 接收回傳的 JSON 並解析出純文字
                    var jsonString = await response.Content.ReadAsStringAsync();
                    using var document = JsonDocument.Parse(jsonString);

                    var reply = document.RootElement
                        .GetProperty("candidates")[0]
                        .GetProperty("content")
                        .GetProperty("parts")[0]
                        .GetProperty("text")
                        .GetString();

                    return reply ?? "AI 沒有產生任何文字。";
                }

                var errorBody = await response.Content.ReadAsStringAsync();
                Console.WriteLine($"[SignalR] Gemini API 錯誤回應: {response.StatusCode} - {errorBody}");
                return $"AI 思考時發生錯誤，狀態碼：{response.StatusCode}";
            }
            catch (Exception ex)
            {
                return $"連線到 AI 發生異常: {ex.Message}";
            }
        }
    }
}
