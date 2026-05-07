using Microsoft.AspNetCore.Mvc;
using PawsPort.ViewModels;
using PawsPort.Dtos;
using PawsPort.Services;
using Serilog;
// 如果有用到 Task，通常需要 using System.Threading.Tasks; 但新版 .NET 預設已包含

namespace PawsPort.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Produces("application/json")]
    public class PassPortController : ApiControllerBase
    {
        private readonly PassPortService _service;

        public PassPortController(PassPortService service)
        {
            _service = service;
        }

        [HttpGet]
        [ProducesResponseType(typeof(IEnumerable<HealthPassportListDto>), StatusCodes.Status200OK)]
        // 1. 加上 async Task<IActionResult>
        public async Task<IActionResult> List([FromQuery] string? keyword)
        {
            // 2. 加上 await，並呼叫 Service 的 Async 方法
            var dtoList = await _service.GetPassportsAsync(keyword);

            if (dtoList == null || !dtoList.Any())
            {
                return NoContent();
            }

            return Success(dtoList, "成功取得列表", 200);
        }

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

        [HttpGet("{id}")]
        public async Task<IActionResult> GetEditData(int id)
        {
            var dto = await _service.GetPassportForEditAsync(id);
            if (dto == null)
                return Failure("PASSPORT_NOT_FOUND", "找不到指定的護照資料", 404);

            return Success(dto, "成功取得編輯資料", 200);
        }

        [HttpPost]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> Create([FromBody] HealthPassportCreateDto dto)
        {
            await _service.CreatePassportAsync(dto);

            Log.Information("創建護照成功 PetId:{PetId}", dto.PetId);

            return Success(dto, "新增資料成功！", 200);
        }

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

        [HttpDelete("{id}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        public async Task<IActionResult> Delete(int id)
        {
            await _service.DeletePassportAsync(id);

            Log.Information("刪除護照成功 PassportId:{PassportId}", id);

            return NoContent();
        }
    }
}