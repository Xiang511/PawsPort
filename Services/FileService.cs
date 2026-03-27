using PawsPort.Models;
using Microsoft.AspNetCore.Http;
using System.IO;


namespace PawsPort.Services
{
    public class FileService
    {

        IWebHostEnvironment _Env = null; //宣告一個變數來存放WebHostEnvironment的實例

        public FileService(IWebHostEnvironment p)
        {
            _Env = p; //將WebHostEnvironment的實例賦值給變數_env，以便在控制器中使用
        }


        // 定義允許的副檔名白名單
        string[] AllowedExtensions = { ".jpg", ".jpeg", ".png", ".gif" };

        public bool IsValidImage(IFormFile File) //驗證上傳的檔案是否為有效的圖片
        {
            if (File == null || File.Length == 0) // 1. 檢查檔案是否為空
                return false;

            // 2. 取得檔案的副檔名
            string Extension = Path.GetExtension(File.FileName).ToLower(); 

            return AllowedExtensions.Contains(Extension); // 3. 檢查副檔名是否在允許的白名單中，如果在則返回true，否則返回false

        }

        public string SaveImage(IFormFile File)
        {

            bool IsValid = IsValidImage(File); // 1. 驗證檔案是否為有效的圖片

            if (IsValid) // 2. 如果檔案有效，則進行儲存
            {
                string ImageName = Guid.NewGuid().ToString() + Path.GetExtension(File.FileName); // 生成唯一的圖片名稱，並保留原始檔案的副檔名
                
                string SavePath = _Env.WebRootPath + "/Images/" + ImageName; // 3. 定義儲存路徑，將圖片儲存在wwwroot/Images資料夾中

                using (FileStream stream = new FileStream(SavePath, FileMode.Create))
                {
                    File.CopyTo(stream); // 4. 使用CopyTo方法將上傳的檔案儲存到指定的路徑，FileMode.Create表示如果檔案已存在則覆蓋
                }
                    

                return ImageName; // 返回生成的圖片名稱
            }
            return null; // 如果檔案無效，返回null
        }
    }
}