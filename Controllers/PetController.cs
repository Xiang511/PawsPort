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
    public class PetController : ApiControllerBase
    {
        private readonly PetService _service;

        public PetController(PetService service)
        {
            _service = service;
        }

        /// <summary>
        /// 取得所有寵物列表
        /// </summary>
        [HttpGet]
        [ProducesResponseType(typeof(IEnumerable<PetListDto>), StatusCodes.Status200OK)]
        public async Task<IActionResult> List([FromQuery] string? keyword)
        {
            var dtoList = await _service.GetPetsAsync(keyword);

            if (dtoList == null || !dtoList.Any())
            {
                return NoContent();
            }

            return Success(dtoList, "成功取得寵物列表", 200);
        }

        /// <summary>
        /// 取得供編輯用的單筆寵物資料
        /// </summary>
        [HttpGet("{id}")]
        public async Task<IActionResult> GetEditData(int id)
        {
            var dto = await _service.GetPetForEditAsync(id);
            if (dto == null)
                return Failure("PET_NOT_FOUND", "找不到指定的寵物資料", 404);

            return Success(dto, "成功取得編輯資料", 200);
        }

        /// <summary>
        /// 創建新寵物資料
        /// </summary>
        [HttpPost]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> Create([FromBody] PetCreateDto dto)
        {
            await _service.CreatePetAsync(dto);
            Log.Information("創建寵物資料成功 Name:{Name}", dto.Name);

            return Success(dto, "新增寵物資料成功！", 200);
        }

        /// <summary>
        /// 更新寵物資料
        /// </summary>
        [HttpPut("{id}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> Edit(int id, [FromBody] PetEditDto dto)
        {
            if (id != dto.PetId)
            {
                return Failure("ID_MISMATCH", "寵物ID不一致", 400);
            }

            await _service.UpdatePetAsync(dto);
            Log.Information("更新寵物資料成功 PetId:{PetId}", id);

            return NoContent();
        }

        /// <summary>
        /// 刪除寵物 (軟刪除)
        /// </summary>
        [HttpDelete("{id}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        public async Task<IActionResult> Delete(int id)
        {
            // 注意這裡對應的是你 Service 裡的 SoftDeletePet
            await _service.SoftDeletePetAsync(id);
            Log.Information("軟刪除寵物資料成功 PetId:{PetId}", id);

            return NoContent();
        }
    }
}