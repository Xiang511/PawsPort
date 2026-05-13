using Microsoft.AspNetCore.Mvc;
using PawsPort.Dtos;
using PawsPort.Services;

namespace PawsPort.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Produces("application/json")]
    [Tags("客服管理")]

    public class ENewsletterController : ApiControllerBase
    {
        private readonly ENewsletterService _newsletterService;

        public ENewsletterController(ENewsletterService newsletterService)
        {
            _newsletterService = newsletterService;
        }


        [HttpGet]
        [Tags("客服管理 / 電子報")]

        public async Task<IActionResult> GetList()
        {
            var result = await _newsletterService.GetAllNewslettersAsync();

            return Success(result, "取得電子報列表成功", 200);
        }


        [HttpPost]
        [Tags("客服管理 / 電子報")]

        public async Task<IActionResult> Create(ENewsletterCreateDTO dto)
        {
            if (!ModelState.IsValid)
            {
                return Failure("VALIDATION_ERROR", "資料格式錯誤", 400);
            }

            var result = await _newsletterService.CreateNewsletterAsync(dto);

            return Success(result, "電子報新增成功", 201);
        }


        [HttpPut("{id}")]
        [Tags("客服管理 / 電子報")]

        public async Task<IActionResult> Update(int id, ENewsletterUpdateDTO dto)
        {
            if (!ModelState.IsValid)
            {
                return Failure("VALIDATION_ERROR", "資料格式錯誤，請檢查必填欄位", 400);
            }


            var isSuccess = await _newsletterService.UpdateNewsletterAsync(id, dto);


            if (!isSuccess)
            {
                return Failure("NEWS_NOT_FOUND", "找不到指定的電子報，更新失敗", 404);
            }


            return Success<object>(null, "電子報修改成功", 200);
        }


        [HttpPatch("{id}")]
        [Tags("客服管理 / 電子報")]

        public async Task<IActionResult> SoftDelete(int id)
        {
            var isSuccess = await _newsletterService.SoftDeleteNewsletterAsync(id);

            if (!isSuccess)
            {
                return Failure("NEWS_NOT_FOUND", "找不到指定的電子報，刪除失敗", 404);
            }


            return Success<object>(null, "電子報刪除成功", 200);
        }
    }
}
