using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PawsPort.Dtos;
using PawsPort.Models;
using PawsPort.Services;
using Serilog;

namespace PawsPort.Controllers
{
    [Authorize(Policy = "遊戲系統_普通管理員")]
    [ApiController]
    [Route("api/[controller]")]
    [Produces("application/json")]
    [Tags("遊戲系統")]
    public class ShopController : ApiControllerBase
    {
        private readonly ShopService _shopService;

        public ShopController(ShopService shopService)
        {
            _shopService = shopService;
        }

        // GET /api/Shop
        /// <summary>
        /// 取得所有商品列表
        /// </summary>
        /// <returns>商城道具與造型列表</returns>
        /// <response code="200">成功取得商品列表</response>
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> List()
        {
            try
            {
                var data = await _shopService.GetShopListAsync();
                return Success(data, "取得商品列表成功", 200);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "ShopController: 取得商品列表時發生異常");
                return Failure("SHOP_LIST_ERROR", "伺服器目前無法讀取商品資料，請稍後再試", 500);
            }
        }

        // POST /api/Shop
        /// <summary>
        /// 新增商城商品
        /// </summary>
        /// <param name="dto">新商品資料（包含名稱、價格等）</param>
        /// <returns>創建成功的商品資料</returns>
        /// <response code="201">成功建立商品</response>
        /// <response code="500">伺服器端儲存失敗</response>
        [HttpPost]
        [ProducesResponseType(typeof(ShopCreateDTO), StatusCodes.Status201Created)]
        public async Task<IActionResult> Create(ShopCreateDTO dto)
        {
            Log.Information("ShopController: 收到 JSON 新增商品請求: {SkinName}", dto.SkinName);

            try
            {
                await _shopService.CreateAsync(dto);
                return Success<object>(dto, "建立成功", 201);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "ShopController: 新增商品失敗");
                return Failure("SHOP_CREATE_FAILED", "伺服器儲存資料失敗", 500);
            }
        }

        // PUT /api/Shop/{id}
        /// <summary>
        /// 更新指定商品資訊
        /// </summary>
        /// <param name="id">商品 ID</param>
        /// <param name="dto">更新的商品資料</param>
        /// <returns>更新後的商品資料</returns>
        /// <response code="200">成功更新商品</response>
        /// <response code="400">網址 ID 與資料 ID 不符</response>
        /// <response code="404">找不到該商品</response>
        [HttpPut("{id}")]
        [ProducesResponseType(typeof(ShopEditDTO), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Edit(int id, ShopEditDTO dto)
        {
            if (id != dto.SkinId) return Failure("SHOP_ID_MISMATCH", "網址 ID 與資料 ID 不符", 400);

            try
            {
                await _shopService.UpdateAsync(dto);
                return Success<object>(dto, "更新成功", 200);
            }
            catch (Exception ex)
            {
                if (ex.Message == "商品不存在")
                    return Failure("SHOP_NOT_FOUND", "找不到指定的商品", 404);

                Log.Error(ex, "ShopController: 更新商品 ID {id} 失敗", id);
                return Failure("SHOP_UPDATE_FAILED", "更新過程發生錯誤", 500);
            }
        }

        // DELETE /api/Shop/{id}
        /// <summary>
        /// 刪除指定商品
        /// </summary>
        /// <param name="id">商品 ID</param>
        /// <response code="200">成功刪除商品</response>
        /// <response code="404">找不到欲刪除的商品</response>
        [HttpDelete("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                await _shopService.DeleteAsync(id);
                return Success<object>(id, "刪除成功", 200);
            }
            catch (Exception ex)
            {
                if (ex.Message == "商品不存在")
                    return Failure("SHOP_NOT_FOUND", "找不到指定的商品", 404);

                Log.Error(ex, "ShopController: 刪除商品 ID {id} 失敗", id);
                return Failure("SHOP_DELETE_FAILED", "刪除過程發生錯誤", 500);
            }
        }
    }
}
