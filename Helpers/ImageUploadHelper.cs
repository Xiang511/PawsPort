using System;
using System.IO;
using System.Text.RegularExpressions;
using Serilog;

namespace PawsPort.Helpers
{
    public static class ImageUploadHelper
    {
        /// <summary>
        /// 將前端傳入的 Base64 圖片解碼並儲存為實體檔案。
        /// 若傳入的已是相對路徑或網址，則會自動跳過不予處理，以相容手動寫入。
        /// </summary>
        public static string SaveBase64Image(string base64String, string subFolder, string webRootPath)
        {
            if (string.IsNullOrWhiteSpace(base64String))
            {
                return null;
            }

            // 1. 防呆檢查：若以 "/" 或 "http" 開頭，或是不包含 base64 標記，代表已是相對/絕對路徑，直接回傳
            if (base64String.StartsWith("/") || base64String.StartsWith("http") || !base64String.Contains(";base64,"))
            {
                return base64String;
            }

            try
            {
                // 2. 解析 Base64 首部取得 MIME 格式
                // 格式範例：data:image/png;base64,iVBORw0KGgoAAAANSUhEUgAA...
                var regex = new Regex(@"^data:image/(?<ext>[a-zA-Z0-9+]+);base64,(?<data>.+)$");
                var match = regex.Match(base64String);

                if (!match.Success)
                {
                    return base64String; // 若不符合標準格式，安全回傳原內容
                }

                string ext = match.Groups["ext"].Value;
                if (ext.ToLower() == "jpeg") ext = "jpg";
                if (ext.Contains("+")) ext = ext.Split('+')[0]; // 例如 svg+xml

                string pureBase64 = match.Groups["data"].Value;
                byte[] imageBytes = Convert.FromBase64String(pureBase64);

                // 3. 確保目標儲存目錄存在
                string targetDir = Path.Combine(webRootPath, "Images", subFolder);
                if (!Directory.Exists(targetDir))
                {
                    Directory.CreateDirectory(targetDir);
                }

                // 4. 產生隨機唯一檔名並儲存
                string fileName = $"{Guid.NewGuid().ToString("N")}.{ext}";
                string physicalPath = Path.Combine(targetDir, fileName);

                File.WriteAllBytes(physicalPath, imageBytes);

                // 5. 回傳資料庫要儲存的相對路徑 URL
                return $"/Images/{subFolder}/{fileName}";
            }
            catch (Exception ex)
            {
                Log.Error(ex, "[ImageUploadHelper] 解析並儲存 Base64 圖片失敗。");
                throw new Exception("圖片儲存處理失敗：" + ex.Message, ex);
            }
        }
    }
}
