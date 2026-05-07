using Microsoft.EntityFrameworkCore;
using PawsPort.Dtos;
using PawsPort.Models;
using Serilog;

namespace PawsPort.Services
{
    public class ShopService
    {
        private readonly PetDbContext _db;
        private readonly IWebHostEnvironment _env;

        public ShopService(PetDbContext db, IWebHostEnvironment env)
        {
            _db = db;
            _env = env;
        }

        // 取得列表
        public async Task<List<ShopListDTO>> GetShopListAsync() =>
            await _db.SkinShops
                .Where(s => s.IsDel != true)
                .Select(s => new ShopListDTO
                {
                    SkinId = s.SkinId,
                    SkinName = s.SkinName,
                    Description = s.Description,
                    Price = s.Price,
                    IsAvailable = s.IsAvailable,
                    SkinImage = s.SkinImage
                }).ToListAsync();

        // 新增商品
        public async Task CreateAsync(ShopCreateDTO dto)
        {
            var skin = new SkinShop
            {
                SkinName = dto.SkinName,
                Description = dto.Description,
                Price = dto.Price,
                IsAvailable = dto.IsAvailable,
                IsDel = false
            };

            if (!string.IsNullOrEmpty(dto.ImageBase64))
            {
                skin.SkinImage = await SaveBase64ImageAsync(dto.ImageBase64);
            }

            _db.SkinShops.Add(skin);
            await _db.SaveChangesAsync();
            Log.Information("ShopService: 商品 '{SkinName}' 新增成功", dto.SkinName);
        }

        // 更新商品
        public async Task UpdateAsync(ShopEditDTO dto)
        {
            var skin = await _db.SkinShops.FirstOrDefaultAsync(s => s.SkinId == dto.SkinId);
            if (skin == null)
            {
                Log.Warning("ShopService: 更新失敗，找不到商品 ID: {SkinId}", dto.SkinId);
                throw new Exception("商品不存在");
            }

            skin.SkinName = dto.SkinName;
            skin.Description = dto.Description;
            skin.Price = dto.Price;
            skin.IsAvailable = dto.IsAvailable;

            if (!string.IsNullOrEmpty(dto.ImageBase64))
            {
                DeleteImage(skin.SkinImage);
                skin.SkinImage = await SaveBase64ImageAsync(dto.ImageBase64);
            }

            await _db.SaveChangesAsync();
            Log.Information("ShopService: 商品 ID: {SkinId} 更新完成", dto.SkinId);
        }

        // 刪除商品
        public async Task DeleteAsync(int id)
        {
            var skin = await _db.SkinShops.FirstOrDefaultAsync(s => s.SkinId == id && s.IsDel != true);
            if (skin == null)
            {
                Log.Warning("ShopService: 刪除失敗，找不到商品 ID: {SkinId}", id);
                throw new Exception("商品不存在");
            }

            DeleteImage(skin.SkinImage);
            skin.IsDel = true;
            await _db.SaveChangesAsync();
            Log.Information("ShopService: 商品 ID: {SkinId} 已刪除", id);
        }

        // --- 非同步檔案處理輔助方法 ---
        private async Task<string> SaveBase64ImageAsync(string base64String)
        {
            if (base64String.Contains(","))
            {
                base64String = base64String.Split(',')[1];
            }

            byte[] imageBytes = Convert.FromBase64String(base64String);
            var fileName = $"{Guid.NewGuid()}.png";
            var uploadPath = Path.Combine(_env.WebRootPath, "images", "skins");

            if (!Directory.Exists(uploadPath)) Directory.CreateDirectory(uploadPath);

            var filePath = Path.Combine(uploadPath, fileName);

            // 寫入檔案
            await File.WriteAllBytesAsync(filePath, imageBytes);

            return $"/images/skins/{fileName}";
        }

        private void DeleteImage(string? path)
        {
            if (string.IsNullOrEmpty(path)) return;
            var fullPath = Path.Combine(_env.WebRootPath, path.TrimStart('/'));
            if (File.Exists(fullPath)) File.Delete(fullPath);
        }
    }
}