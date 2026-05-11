using Microsoft.AspNetCore.Mvc;
using PawsPort.Dtos;
using PawsPort.Responses;
using PawsPort.Services;
using Serilog;

namespace PawsPort.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Produces("application/json")]
    [Tags("客服管理")]

    public class SupportController : ApiControllerBase
    {

        private readonly FaqService _faqService;
        private readonly QaService _qaService;

        public SupportController(FaqService faqService, QaService qaService)
        {
            _faqService = faqService;
            _qaService = qaService;
        }

        //List
        [HttpGet("Faq")]
        [ProducesResponseType(typeof(ApiResponse<List<FaqDTO>>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [Tags("客服管理 / 常見問題")]

        public async Task<IActionResult> GetFaqs()
        {
            var faqs = await _faqService.GetAllFaqsAsync();

            if (faqs == null || !faqs.Any())
            {
                return NoContent();
            }

            return Success(faqs, "成功取得FAQ列表", 200);
        }


        [HttpPost("Faq")]
        [ProducesResponseType(typeof(ApiResponse<FaqDTO>), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [Tags("客服管理 / 常見問題")]

        public async Task<IActionResult> CreateFaq(FaqCreateDTO dto)
        {
            if (!ModelState.IsValid)
            {
                return Failure("VALIDATION_ERROR", "資料驗證失敗，請檢查必填欄位", 400);
            }

            var result = await _faqService.CreateFaqAsync(dto);

            Log.Information("新增FAQ成功 FAQ_ID:{Faqid}", result.Faqid);

            return Success(result, "創建FAQ成功", 201);
        }


        [HttpPut("Faq/{id}")]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [Tags("客服管理 / 常見問題")]

        public async Task<IActionResult> UpdateFaq(int id, FaqUpdateDTO dto)
        {
            if (!ModelState.IsValid)
            {
                return Failure("VALIDATION_ERROR", "資料驗證失敗，請檢查欄位", 400);
            }

            var isSuccess = await _faqService.UpdateFaqAsync(id, dto);

            if (!isSuccess)
            {
                return Failure("FAQ_NOT_FOUND", "找不到指定的FAQ，更新失敗", 404);
            }

            return Success<object>(null, "更新FAQ成功", 200);
        }


        [HttpPatch("Faq/{id}")]
        [Tags("客服管理 / 常見問題")]

        public async Task<IActionResult> SoftDeleteFaq(int id)
        {
            var result = await _faqService.SoftDeleteFaqAsync(id);

            if (!result)
            {
                return Failure("FAQ_NOT_FOUND", "找不到指定的FAQ", 404);
            }

            return NoContent();
        }




        [HttpGet("Qa")]
        [Tags("客服管理 / QA記錄")]

        public async Task<IActionResult> GetQaList()
        {
            var result = await _qaService.GetAllQaAsync();
            return Success(result, "取得QA列表成功", 200);
        }


        //取得單筆QA明細
        [HttpGet("Qa/{id}")]
        [Tags("客服管理 / QA記錄")]

        public async Task<IActionResult> GetQaDetails(int id)
        {
            var result = await _qaService.GetQaByIdAsync(id);
            if (result == null)
            {
                return Failure("QA_NOT_FOUND", "找不到指定的問答紀錄", 404);
            }
            return Success(result, "取得QA明細成功", 200);
        }


        [HttpPut("Qa/{id}")]
        [Tags("客服管理 / QA記錄")]

        public async Task<IActionResult> UpdateQa(int id, QaUpdateDTO dto)
        {
            if (!ModelState.IsValid)
            {
                return Failure("VALIDATION_ERROR", "資料格式錯誤", 400);
            }

            var isSuccess = await _qaService.UpdateQaAsync(id, dto);
            if (!isSuccess)
            {
                return Failure("QA_NOT_FOUND", "找不到該筆問答紀錄，更新失敗", 404);
            }

            return Success<object>(null, "QA回覆成功", 200);
        }
    }
}
