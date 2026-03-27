using Microsoft.AspNetCore.Mvc;
using PawsPort.Models;

namespace PawsPort.Controllers
{
    public class ShopController : Controller
    {
        public IActionResult List()
        {
            PetDbContext db = new PetDbContext();
            var skinList = db.SkinShops.Where(s => s.IsDel != true).ToList();
            return View(skinList);
        }
        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        public IActionResult Create(SkinShop S, IFormFile imageFile)
        {
            try
            {
                // 驗證圖片
                if (imageFile != null && imageFile.Length > 0)
                {
                    // 檢查文件格式
                    var allowedExtensions = new[] { ".jpg", ".jpeg", ".png" };
                    var fileExtension = Path.GetExtension(imageFile.FileName).ToLower();

                    if (!allowedExtensions.Contains(fileExtension))
                    {
                        ModelState.AddModelError("imageFile", "只允許上傳 JPG 或 PNG 格式的圖片");
                        return View(S);
                    }

                    // 生成唯一的文件名
                    var fileName = $"{Guid.NewGuid()}{fileExtension}";
                    var uploadPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "images", "skins");

                    // 確保資料夾存在
                    if (!Directory.Exists(uploadPath))
                    {
                        Directory.CreateDirectory(uploadPath);
                    }

                    var filePath = Path.Combine(uploadPath, fileName);

                    // 保存文件
                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        imageFile.CopyTo(stream);
                    }

                    // 設置完整的 URL 路徑
                    S.SkinImage = $"/images/skins/{fileName}";
                }

                S.IsDel = false;

                PetDbContext db = new PetDbContext();
                db.SkinShops.Add(S);
                db.SaveChanges();

                return RedirectToAction("List");
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", $"保存失敗: {ex.Message}");
                return View(S);
            }
        }
        public IActionResult Edit(int? id)
        {
            PetDbContext db = new PetDbContext();
            SkinShop shop = db.SkinShops.FirstOrDefault(shop => shop.SkinId == id);
            if (shop == null)
                return RedirectToAction("List");
            return View(shop);
        }

        [HttpPost]
        public IActionResult Edit(SkinShop S, IFormFile imageFile)
        {
            try
            {
                PetDbContext db = new PetDbContext();
                SkinShop ExistingSkin = db.SkinShops.FirstOrDefault(s => s.SkinId == S.SkinId);

                if (ExistingSkin != null)
                {
                    ExistingSkin.SkinName = S.SkinName;
                    ExistingSkin.Description = S.Description;
                    ExistingSkin.Price = S.Price;
                    ExistingSkin.IsAvailable = S.IsAvailable;

                    // 如果上傳了新圖片
                    if (imageFile != null && imageFile.Length > 0)
                    {
                        // 檢查文件格式
                        var allowedExtensions = new[] { ".jpg", ".jpeg", ".png" };
                        var fileExtension = Path.GetExtension(imageFile.FileName).ToLower();

                        if (!allowedExtensions.Contains(fileExtension))
                        {
                            ModelState.AddModelError("imageFile", "只允許上傳 JPG 或 PNG 格式的圖片");
                            return View(S);
                        }

                        // 刪除舊圖片
                        if (!string.IsNullOrEmpty(ExistingSkin.SkinImage))
                        {
                            var oldImagePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", ExistingSkin.SkinImage.TrimStart('/'));
                            if (System.IO.File.Exists(oldImagePath))
                            {
                                System.IO.File.Delete(oldImagePath);
                            }
                        }

                        // 生成唯一的文件名
                        var fileName = $"{Guid.NewGuid()}{fileExtension}";
                        var uploadPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "images", "skins");

                        if (!Directory.Exists(uploadPath))
                        {
                            Directory.CreateDirectory(uploadPath);
                        }

                        var filePath = Path.Combine(uploadPath, fileName);

                        // 保存文件
                        using (var stream = new FileStream(filePath, FileMode.Create))
                        {
                            imageFile.CopyTo(stream);
                        }

                        // 設置完整的 URL 路徑
                        ExistingSkin.SkinImage = $"/images/skins/{fileName}";
                    }

                    db.SaveChanges();
                }

                return RedirectToAction("List");
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", $"保存失敗: {ex.Message}");
                return View(S);
            }
        }


        [HttpPost]
        public IActionResult Delete(int id)
        {
            try
            {
                PetDbContext db = new PetDbContext();
                SkinShop skin = db.SkinShops.FirstOrDefault(s => s.SkinId == id && s.IsDel != true);

                if (skin != null)
                {
                    // 刪除圖片文件
                    if (!string.IsNullOrEmpty(skin.SkinImage))
                    {
                        var imagePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", skin.SkinImage.TrimStart('/'));
                        if (System.IO.File.Exists(imagePath))
                        {
                            System.IO.File.Delete(imagePath);
                        }
                    }

                    // 軟刪除
                    skin.IsDel = true;
                    db.SaveChanges();
                }

                return RedirectToAction("List");
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"刪除失敗: {ex.Message}";
                return RedirectToAction("List");
            }
        }



    }
}
