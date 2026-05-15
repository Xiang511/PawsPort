using Microsoft.AspNetCore.Mvc;
using PawsPort.ViewModels; 
using PawsPort.DTOs;
using PawsPort.Services;
using Serilog;

namespace PawsPort.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Produces("application/json")]
    [Tags("寵物管理")]
    public class AdoptionRecordController : ApiControllerBase
    {
        private readonly AdoptionRecordService _service;

        public AdoptionRecordController(AdoptionRecordService service)
        {
            _service = service;
        }

        /// <summary>
        /// 取得所有領養紀錄列表
        /// </summary>
        [HttpGet]
        [ProducesResponseType(typeof(IEnumerable<AdoptionRecordListDto>), StatusCodes.Status200OK)]
        public async Task<IActionResult> List([FromQuery] string? keyword)
        {
            var dtoList = await _service.GetAdoptionRecordsAsync(keyword);

            if (dtoList == null || !dtoList.Any())
            {
                return NoContent();
            }

            return Success(dtoList, "成功取得領養紀錄列表", 200);
        }

        /// <summary>
        /// 取得供編輯用的單筆領養資料
        /// </summary>
        [HttpGet("{id}")]
        public async Task<IActionResult> GetEditData(int id)
        {
            var dto = await _service.GetRecordForEditAsync(id);
            if (dto == null)
                return Failure("RECORD_NOT_FOUND", "找不到指定的領養紀錄", 404);

            return Success(dto, "成功取得編輯資料", 200);
        }

        /// <summary>
        /// 創建新領養紀錄
        /// </summary>
        [HttpPost]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> Create([FromBody] AdoptionRecordCreateDto dto)
        {
            await _service.CreateRecordAsync(dto);
            Log.Information("創建領養紀錄成功 PetId:{PetId}, UserId:{UserId}", dto.PetId, dto.UserId);

            return Success(dto, "新增領養紀錄成功！", 200);
        }

        /// <summary>
        /// 更新領養紀錄
        /// </summary>
        [HttpPut("{id}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> Edit(int id, [FromBody] AdoptionRecordEditDto dto)
        {
            if (id != dto.AdoptionId)
            {
                return Failure("ID_MISMATCH", "領養紀錄ID不一致", 400);
            }

            await _service.UpdateRecordAsync(dto);
            Log.Information("更新領養紀錄成功 AdoptionId:{AdoptionId}", id);

            return NoContent();
        }

        /// <summary>
        /// 軟刪除領養紀錄 (將紀錄標記為已刪除)
        /// </summary>
        /// <param name="id">領養紀錄 ID</param>
        [HttpPatch("{id}")] 
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        public async Task<IActionResult> SoftDelete(int id) //方法名同步修改
        {
            // 呼叫 Service 執行軟刪除邏輯
            await _service.DeleteRecordAsync(id);

            Log.Information("軟刪除領養紀錄成功 AdoptionId:{AdoptionId}", id);

            // 雖然是更新，但因為沒有要回傳新資料，回傳 204 No Content 依然是標準作法
            return NoContent();
        }
    }
}