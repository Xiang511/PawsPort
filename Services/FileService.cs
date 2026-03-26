using PawsPort.Models;
using Microsoft.AspNetCore.Http;
using System.IO;


namespace PawsPort.Services
{
    public class FileService
    {

        // 1. 定義允許的副檔名白名單
        string[] allowedExtensions = { ".jpg", ".jpeg", ".png", ".gif" };

        public bool IsValidImage(IFormFile File) //驗證上傳的檔案是否為有效的圖片
        {
            if (File == null || File.Length == 0) // 1. 檢查檔案是否為空
                return false;

            // 2. 取得檔案的副檔名
            string Extension = Path.GetExtension(File.FileName).ToLower(); // 3. 檢查副檔名是否在允許的白名單中

            return allowedExtensions.Contains(Extension);

        }
    }
}