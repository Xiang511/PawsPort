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

        public List<ShopListDTO> GetShopList() => _db.SkinShops
            .Where(s => s.IsDel != true)
            .Select(s => new ShopListDTO
            {
                SkinId = s.SkinId,
                SkinName = s.SkinName,
                Description = s.Description,
                Price = s.Price,
                IsAvailable = s.IsAvailable,
                SkinImage = s.SkinImage
            }).ToList();

        public void Create(ShopCreateDTO dto)
        {
            var skin = new SkinShop
            {
                SkinName = dto.SkinName,
                Description = dto.Description,
                Price = dto.Price,
                IsAvailable = dto.IsAvailable,
                IsDel = false
            };
            // 檢查 JSON 傳進來的 Base64 字串
            if (!string.IsNullOrEmpty(dto.ImageBase64))
            {
                skin.SkinImage = SaveBase64Image(dto.ImageBase64);
            }

            _db.SkinShops.Add(skin);
            _db.SaveChanges();
            Log.Information("ShopService: 商品 '{SkinName}' 新增成功", dto.SkinName);
        }

        public void Update(ShopEditDTO dto)
        {
            var skin = _db.SkinShops.FirstOrDefault(s => s.SkinId == dto.SkinId);
            if (skin == null)
            {
                Log.Information("ShopService: 更新失敗，找不到商品 ID: {SkinId}", dto.SkinId);
                throw new Exception("商品不存在");
            }

            skin.SkinName = dto.SkinName;
            skin.Description = dto.Description;
            skin.Price = dto.Price;
            skin.IsAvailable = dto.IsAvailable;

            // 檢查是否更新圖片
            if (!string.IsNullOrEmpty(dto.ImageBase64))
            {
                DeleteImage(skin.SkinImage);
                skin.SkinImage = SaveBase64Image(dto.ImageBase64);
            }

            _db.SaveChanges();
            Log.Information("ShopService: 商品 ID: {SkinId} 更新完成", dto.SkinId);
        }

        public void Delete(int id)
        {
            var skin = _db.SkinShops.FirstOrDefault(s => s.SkinId == id && s.IsDel != true);
            if (skin == null)
            {
                Log.Information("ShopService: 刪除失敗，找不到商品 ID: {SkinId}", id);
                throw new Exception("商品不存在");
            }

            DeleteImage(skin.SkinImage);
            skin.IsDel = true;
            _db.SaveChanges();
            Log.Information("ShopService: 商品 ID: {SkinId} 已刪除", id);
        }

        // --- 檔案處理輔助方法 ---
        private string SaveBase64Image(string base64String)
        {
            // 處理前端可能帶有的 data:image/png;base64, 前綴
            if (base64String.Contains(","))
            {
                base64String = base64String.Split(',')[1];
            }

            // 將 Base64 字串轉為 Byte 陣列
            byte[] imageBytes = Convert.FromBase64String(base64String);

            // 生成檔案名稱
            var fileName = $"{Guid.NewGuid()}.png";
            var uploadPath = Path.Combine(_env.WebRootPath, "images", "skins");

            if (!Directory.Exists(uploadPath)) Directory.CreateDirectory(uploadPath);

            // 直接寫入檔案
            var filePath = Path.Combine(uploadPath, fileName);
            File.WriteAllBytes(filePath, imageBytes);

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