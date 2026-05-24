using Microsoft.AspNetCore.Mvc;
using PawsPort.Services;
using Microsoft.AspNetCore.Http;

namespace PawsPort.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Produces("application/json")]
    public class FileController : ApiControllerBase
    {
        private readonly FileService _fileService;

        //注入 FileService
        public FileController(FileService fileService)
        {
            _fileService = fileService;
        }

        /// <summary>
        /// 提供給 Quill 編輯器上傳圖片的專用接口
        /// </summary>
        [HttpPost("upload")]
        [Tags("社群管理")]
        public async Task<IActionResult> Upload([FromForm] IFormFile file) 
        {
            if (file == null)
            {
                return BadRequest(new { message = "未接收到檔案" });
            }

            // 呼叫 Service 進行驗證、存檔，並傳入目前請求的 HttpContext.Request 組合網址
            string imageUrl = await _fileService.SaveImageForQuillAsync(file, HttpContext.Request);

            if (string.IsNullOrEmpty(imageUrl))
            {
                return BadRequest(new { message = "檔案格式不正確或圖片無效" });
            }

            // 成功後回傳 { url: "https://..." }是 Quill 認得的格式
            return Ok(new { url = imageUrl });
        }

    }
}

