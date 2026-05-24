using PawsPort.Models;
using Microsoft.AspNetCore.Http;
using System.IO;
using Microsoft.AspNetCore.Hosting;


namespace PawsPort.Services
{
    public class FileService
    {
        private readonly IWebHostEnvironment _Env;
        public FileService(IWebHostEnvironment p)
        {
            _Env = p;
        }

        // 定義允許的副檔名白名單
        private readonly string[] AllowedExtensions = { ".jpg", ".jpeg", ".png", ".gif", ".webp" };

        public bool IsValidImage(IFormFile File) //驗證上傳的檔案是否為有效的圖片
        {
            if (File == null || File.Length == 0)
                return false;

            string Extension = Path.GetExtension(File.FileName).ToLower();
            return AllowedExtensions.Contains(Extension);
        }



        // 配合 Quill 上傳圖片，改為非同步並傳入 HttpRequest 組合完整網址

        public async Task<string> SaveImageForQuillAsync(IFormFile File, HttpRequest Request)
        {
            bool IsValid = IsValidImage(File); // 1. 驗證檔案是否為有效的圖片

            try
            {
                if (File == null || File.Length == 0 || !IsValid)
                {
                    return string.Empty; // 失敗時回傳空字串，絕不回傳 null
                }
                //之後可以加上去擋太大的圖片
                //if (File.Length > 5 * 1024 * 1024)
                //{
                //    return string.Empty;
                //}

                // 2. 確保 wwwroot/articleimages 資料夾存在
                string ImagesFolder = Path.Combine(_Env.WebRootPath,"Images","articleimages");
              
                if (!Directory.Exists(ImagesFolder))
                {
                    Directory.CreateDirectory(ImagesFolder);
                }

                // 3. 生成唯一的圖片名稱（使用 GUID）
                string ImageName = Guid.NewGuid().ToString() + Path.GetExtension(File.FileName);

                // 4. 組合安全路徑
                string SavePath = Path.Combine(ImagesFolder, ImageName);

                // 5. 儲存檔案
                await using (FileStream stream = new FileStream(SavePath, FileMode.Create))
                {
                    await File.CopyToAsync(stream);
                }

                string baseUrl = $"{Request.Scheme}://{Request.Host}";
                string returnUrl = $"{baseUrl}/Images/articleimages/{ImageName}";

                return returnUrl;
            }
            catch (Exception ex)
            {
                // 如果真的有其他意外（例如硬碟滿了），會安全地記錄在這裡，不會讓伺服器暴斃
                Console.WriteLine($"====== 圖片儲存失敗 ======: {ex.Message}");
                return string.Empty;
            }
        }

    }
}


