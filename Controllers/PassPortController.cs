using Microsoft.AspNetCore.Mvc;
using PawsPort.Dtos;
using PawsPort.Services;
using Serilog;

namespace PawsPort.Controllers
{
    /// <summary>
    /// 寵物健康護照管理 API
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    [Produces("application/json")]
    [Tags("寵物管理")]
    public class PassPortController : ApiControllerBase
    {
        private readonly PassPortService _service;

        public PassPortController(PassPortService service)
        {
            _service = service;
        }

        /// <summary>
        /// 取得寵物健康護照列表 (可根據寵物名稱進行過濾)
        /// </summary>
        /// <param name="keyword">搜尋關鍵字 (寵物名稱)</param>
        /// <returns>回傳符合條件的護照清單</returns>
        /// <response code="200">成功取得列表</response>
        /// <response code="204">成功處理請求，但查無任何資料</response>
        [HttpGet]
        [ProducesResponseType(typeof(IEnumerable<HealthPassportListDto>), StatusCodes.Status200OK)]
        public async Task<IActionResult> List([FromQuery] string? keyword)
        {
            var dtoList = await _service.GetPassportsAsync(keyword);

            if (dtoList == null || !dtoList.Any())
            {
                return NoContent();
            }

            return Success(dtoList, "成功取得列表", 200);
        }

        /// <summary>
        /// 取得特定寵物的健康護照詳細資訊 (包含病歷與疫苗紀錄)
        /// </summary>
        /// <param name="id">護照 ID</param>
        /// <returns>回傳詳細的健康紀錄資料</returns>
        /// <response code="200">成功取得詳細資料</response>
        /// <response code="404">找不到指定的護照資料</response>
        [HttpGet("{id}/detail")]
        [ProducesResponseType(typeof(HealthPassportDetailsDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Details(int id)
        {
            var dto = await _service.GetPassportDetailsAsync(id);

            if (dto == null)
            {
                return Failure("PASSPORT_NOT_FOUND", "該名寵物目前沒有任何病歷與疫苗資料喔！", 404);
            }

            return Success(dto, "成功取得詳細資料", 200);
        }

        /// <summary>
        /// 取得編輯所需的護照原始資料 (後台表單綁定用)
        /// </summary>
        /// <param name="id">護照 ID</param>
        /// <returns>回傳供編輯使用的 DTO</returns>
        /// <response code="200">成功取得資料</response>
        /// <response code="404">找不到指定的護照資料</response>
        [HttpGet("{id}")]
        [ProducesResponseType(typeof(HealthPassportEditDto), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetEditData(int id)
        {
            var dto = await _service.GetPassportForEditAsync(id);
            if (dto == null)
                return Failure("PASSPORT_NOT_FOUND", "找不到指定的護照資料", 404);

            return Success(dto, "成功取得編輯資料", 200);
        }

        /// <summary>
        /// 建立新的寵物健康護照
        /// </summary>
        /// <remarks>
        /// 此 API 會同時建立護照主檔，並根據輸入內容新增病歷與疫苗紀錄。
        /// </remarks>
        /// <param name="dto">護照建立 DTO</param>
        /// <returns>回傳新增成功的結果與資料</returns>
        /// <response code="200">新增資料成功</response>
        [HttpPost]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> Create([FromBody] HealthPassportCreateDto dto)
        {
            await _service.CreatePassportAsync(dto);

            Log.Information("創建護照成功 PetId:{PetId}", dto.PetId);

            return Success(dto, "新增資料成功！", 200);
        }

        /// <summary>
        /// 更新特定健康護照的內容
        /// </summary>
        /// <param name="id">路徑中的護照 ID</param>
        /// <param name="dto">包含更新資訊的 DTO</param>
        /// <returns>回傳 204 NoContent 表示更新成功</returns>
        /// <response code="204">更新成功</response>
        /// <response code="400">ID 不一致或請求格式錯誤</response>
        [HttpPut("{id}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> Edit(int id, [FromBody] HealthPassportEditDto dto)
        {
            if (id != dto.PassportId)
            {
                return Failure("ID_MISMATCH", "護照ID不一致", 400);
            }

            await _service.UpdatePassportAsync(dto);

            Log.Information("更新護照成功 PassportId:{PassportId}", id);

            return NoContent();
        }

        /// <summary>
        /// 軟刪除指定健康護照
        /// </summary>
        /// <remarks>
        /// 將資料標記為刪除狀態，不會從資料庫中物理移除。
        /// </remarks>
        /// <param name="id">欲刪除的護照 ID</param>
        /// <returns>回傳 204 NoContent 表示刪除成功</returns>
        /// <response code="204">軟刪除成功</response>
        [HttpPatch("{id}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        public async Task<IActionResult> SoftDelete(int id)
        {
            await _service.DeletePassportAsync(id);

            Log.Information("刪除護照成功 PassportId:{PassportId}", id);

            return NoContent();
        }
    }
}
