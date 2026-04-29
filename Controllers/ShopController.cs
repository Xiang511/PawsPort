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

        public ShopController(ShopService shopService) => _shopService = shopService;

        [HttpGet]
        public IActionResult List() => Success(_shopService.GetShopList(), "取得成功", 200);

        [HttpPost]
        public IActionResult Create(ShopCreateDTO dto)
        {
            Log.Information("ShopController: 收到 JSON 新增商品請求: {SkinName}", dto.SkinName);
            // 圖片驗證檢查

            try
            {
                _shopService.Create(dto);
                return Success<object>(null, "建立成功", 201);
            }
            catch (Exception ex)
            {
                Log.Information(ex, "ShopController: 新增商品失敗");
                return Failure("SHOP_CREATE_FAILED", "伺服器儲存資料失敗", 500);
            }
        }

        [HttpPut("{id}")]
        public IActionResult Edit(int id, ShopEditDTO dto)
        {
            // 1. 參數驗證失敗 (400)
            if (id != dto.SkinId) return Failure("SHOP_ID_MISMATCH", "網址 ID 與資料 ID 不符", 400);

            // 2. 圖片驗證失敗 (400)
            
            try
            {
                _shopService.Update(dto);
                return Success<object>(null, "更新成功", 200);
            }
            catch (Exception ex)
            {
                if (ex.Message == "商品不存在")
                    return Failure("SHOP_NOT_FOUND", "找不到指定的商品", 404);

                Log.Error(ex, "ShopController: 更新商品 ID {id} 失敗", id);
                return Failure("SHOP_UPDATE_FAILED", "更新過程發生錯誤", 500);
            }
        }

        [HttpDelete("{id}")]
        public IActionResult Delete(int id)
        {
            try
            {
                _shopService.Delete(id);
                return Success<object>(null, "刪除成功", 200);
            }
            catch (Exception ex)
            {
                // 找不到資料回 404
                if (ex.Message == "商品不存在")
                    return Failure("SHOP_NOT_FOUND", "找不到指定的商品", 404);

                Log.Error(ex, "ShopController: 刪除商品 ID {id} 失敗", id);
                return Failure("SHOP_DELETE_FAILED", "刪除過程發生錯誤", 500);
            }
        }
    }
}
