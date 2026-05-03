using Microsoft.AspNetCore.Mvc;
using PawsPort.Dtos;
using PawsPort.Models;
using PawsPort.Services;
using Serilog;

namespace PawsPort.Controllers
{
    [Route("api/[controller]")]
    public class ShopController : ApiControllerBase
    {
        private readonly ShopService _shopService;

        public ShopController(ShopService shopService)
        {
            _shopService = shopService;
        }

        // GET /api/Shop
        [HttpGet]
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
        [HttpPost]
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
        [HttpPut("{id}")]
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
        [HttpDelete("{id}")]
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
