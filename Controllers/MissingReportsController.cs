using Microsoft.AspNetCore.Mvc;
using PawsPort.Dtos;
using PawsPort.Services;
using Serilog;

namespace PawsPort.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Produces("application/json")]
    [Tags("寵物管理")]
    public class MissingReportsController : ApiControllerBase
    {
        private readonly MissingReportsService _service;

        public MissingReportsController(MissingReportsService service)
        {
            _service = service;
        }

        /// <summary>
        /// 取得失蹤報案列表
        /// </summary>
        [HttpGet]
        [ProducesResponseType(typeof(IEnumerable<MissingReportListDto>), StatusCodes.Status200OK)]
        public async Task<IActionResult> List([FromQuery] string? keyword)
        {
            var dtoList = await _service.GetReportsAsync(keyword);

            if (dtoList == null || !dtoList.Any())
            {
                return NoContent();
            }

            return Success(dtoList, "成功取得報案列表", 200);
        }

        /// <summary>
        /// 取得供編輯用的單筆報案資料
        /// </summary>
        [HttpGet("{id}")]
        public async Task<IActionResult> GetEditData(int id)
        {
            var dto = await _service.GetReportForEditAsync(id);
            if (dto == null)
                return Failure("REPORT_NOT_FOUND", "找不到指定的報案資料", 404);

            return Success(dto, "成功取得編輯資料", 200);
        }

        /// <summary>
        /// 創建新失蹤報案
        /// </summary>
        [HttpPost]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> Create([FromBody] MissingReportCreateDto dto)
        {
            await _service.CreateReportAsync(dto);
            Log.Information("創建失蹤報案成功 PetId:{PetId}", dto.PetId);

            return Success(dto, "新增報案成功！", 200);
        }

        /// <summary>
        /// 更新失蹤報案資料
        /// </summary>
        [HttpPut("{id}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> Edit(int id, [FromBody] MissingReportEditDto dto)
        {
            if (id != dto.ReportId)
            {
                return Failure("ID_MISMATCH", "報案ID不一致", 400);
            }

            await _service.UpdateReportAsync(dto);
            Log.Information("更新報案成功 ReportId:{ReportId}", id);

            return NoContent();
        }

        /// <summary>
        /// 刪除失蹤報案
        /// </summary>
        /// <remarks>
        /// 將資料標記為刪除狀態，不會從資料庫中物理移除。
        /// </remarks>
        /// <param name="id">欲刪除的報案 ID</param>
        /// <returns>回傳 204 NoContent 表示刪除成功</returns>
        /// <response code="204">軟刪除成功</response>
        [HttpPatch("{id}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        public async Task<IActionResult> Delete(int id)
        {
            await _service.DeleteReportAsync(id);
            Log.Information("刪除報案成功 ReportId:{ReportId}", id);

            return NoContent();
        }
    }
}
