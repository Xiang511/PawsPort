using Microsoft.AspNetCore.Mvc;
using PawsPort.ViewModels; 
using PawsPort.Dtos;
using PawsPort.Services;
using Serilog;

namespace PawsPort.Controllers
{
    // 1. 加上 API 標籤與路由
    [ApiController]
    [Route("api/[controller]")]
    [Produces("application/json")]
    public class PassPortController : ApiControllerBase // 2. 繼承 ApiControllerBase
    {
        private readonly PassPortService _service;

        public PassPortController(PassPortService service)
        {
            _service = service;
        }

        /// <summary>
        /// 取得所有健康護照列表
        /// </summary>
        /// <param name="keyword">搜尋關鍵字 (寵物名稱)</param>
        /// <returns>護照列表 JSON</returns>
        [HttpGet]
        [ProducesResponseType(typeof(IEnumerable<HealthPassportListDto>), StatusCodes.Status200OK)]
        public IActionResult List([FromQuery] string? keyword) // 用 [FromQuery] 接收搜尋參數
        {
            var dtoList = _service.GetPassports(keyword);

            if (dtoList == null || !dtoList.Any())
            {
                return NoContent(); // HTTP 204: 成功處理，但沒有資料
            }

            return Success(dtoList, "成功取得列表", 200);
        }

        /// <summary>
        /// 取得單筆健康護照詳細資料 (給前台查看用)
        /// </summary>
        [HttpGet("{id}")]
        [ProducesResponseType(typeof(HealthPassportDetailsDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public IActionResult Details(int id)
        {
            var dto = _service.GetPassportDetails(id);

            if (dto == null)
            {
                // 用 Failure 回傳自訂錯誤格式
                return Failure("PASSPORT_NOT_FOUND", "該名寵物目前沒有任何病歷與疫苗資料喔！", 404);
            }

            return Success(dto, "成功取得詳細資料", 200);
        }

        /// <summary>
        /// 取得供編輯用的單筆護照資料 (給後台表單綁定用)
        /// </summary>
        [HttpGet("{id}/edit")]
        public IActionResult GetEditData(int id)
        {
            var dto = _service.GetPassportForEdit(id);
            if (dto == null)
                return Failure("PASSPORT_NOT_FOUND", "找不到指定的護照資料", 404);

            return Success(dto, "成功取得編輯資料", 200);
        }

        /// <summary>
        /// 創建新健康護照
        /// </summary>
        [HttpPost]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public IActionResult Create([FromBody] HealthPassportCreateDto dto) // 用 [FromBody] 接收 JSON
        {
            _service.CreatePassport(dto);

            Log.Information("創建護照成功 PetId:{PetId}", dto.PetId);

            // 新增成功，不一定需要回傳整包資料，可以只回傳成功訊息
            return Success(dto, "新增資料成功！", 200);
        }

        /// <summary>
        /// 更新護照資料
        /// </summary>
        [HttpPut("{id}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public IActionResult Edit(int id, [FromBody] HealthPassportEditDto dto)
        {
            // 防呆：確認網址列的 ID 跟傳進來的 DTO ID 是一致的
            if (id != dto.PassportId)
            {
                return Failure("ID_MISMATCH", "護照ID不一致", 400);
            }

            _service.UpdatePassport(dto);

            Log.Information("更新護照成功 PassportId:{PassportId}", id);

            return NoContent(); // 修改成功通常回傳 204
        }

        /// <summary>
        /// 刪除護照
        /// </summary>
        [HttpDelete("{id}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        public IActionResult Delete(int id)
        {
            _service.DeletePassport(id);

            Log.Information("刪除護照成功 PassportId:{PassportId}", id);

            return NoContent();
        }
    }
}