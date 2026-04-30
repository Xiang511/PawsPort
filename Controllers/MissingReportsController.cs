using Microsoft.AspNetCore.Mvc;
using PawsPort.ViewModels;
using PawsPort.Dtos;
using PawsPort.Services;
using Serilog;

namespace PawsPort.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Produces("application/json")]
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
        public IActionResult List([FromQuery] string? keyword)
        {
            var dtoList = _service.GetReports(keyword);

            if (dtoList == null || !dtoList.Any())
            {
                return NoContent();
            }

            return Success(dtoList, "成功取得報案列表", 200);
        }

        /// <summary>
        /// 取得供編輯用的單筆報案資料
        /// </summary>
        [HttpGet("{id}/edit")]
        public IActionResult GetEditData(int id)
        {
            var dto = _service.GetReportForEdit(id);
            if (dto == null)
                return Failure("REPORT_NOT_FOUND", "找不到指定的報案資料", 404);

            return Success(dto, "成功取得編輯資料", 200);
        }

        /// <summary>
        /// 創建新失蹤報案
        /// </summary>
        [HttpPost]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public IActionResult Create([FromBody] MissingReportCreateDto dto)
        {
            _service.CreateReport(dto);
            Log.Information("創建失蹤報案成功 PetId:{PetId}", dto.PetId);

            return Success(dto, "新增報案成功！", 200);
        }

        /// <summary>
        /// 更新失蹤報案資料
        /// </summary>
        [HttpPut("{id}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public IActionResult Edit(int id, [FromBody] MissingReportEditDto dto)
        {
            if (id != dto.ReportId)
            {
                return Failure("ID_MISMATCH", "報案ID不一致", 400);
            }

            _service.UpdateReport(dto);
            Log.Information("更新報案成功 ReportId:{ReportId}", id);

            return NoContent();
        }

        /// <summary>
        /// 刪除失蹤報案
        /// </summary>
        [HttpDelete("{id}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        public IActionResult Delete(int id)
        {
            _service.DeleteReport(id);
            Log.Information("刪除報案成功 ReportId:{ReportId}", id);

            return NoContent();
        }
    }
}