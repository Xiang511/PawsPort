using Microsoft.AspNetCore.Mvc;
using PawsPort.Models;
using PawsPort.ViewModels;

namespace PawsPort.Controllers
{
    public class PassPortController : Controller
    {
        public IActionResult List(KeywordViewModel vm)
        {
            PetDbContext db = new PetDbContext();
            IEnumerable<HealthPassport> datas = null;

            if (string.IsNullOrEmpty(vm.txtKeyword))
            {
                // 關鍵字為空時：抓出所有資料，但要「過濾掉」已經有刪除時間的
                datas = from p in db.HealthPassports
                        where p.CreatedAt != null   // 👈 新增這個條件
                        select p;
            }
            else
            {
                // 有關鍵字時：一樣要加上 DeletedAt == null 的條件
                datas = (IEnumerable<HealthPassport>?)db.Pets.Where(p =>
                    p.DeletedAt == null &&          // 👈 新增這個條件
                    (p.Name.Contains(vm.txtKeyword) || p.CoatColor.Contains(vm.txtKeyword))
                );
            }

            return View(datas);
        }

        public IActionResult Create()
        {
            return View();
        }
        [HttpPost]
        public IActionResult Create(HealthPassport p)
        {
            PetDbContext db = new PetDbContext();
            p.PetId = 0;
            db.HealthPassports.Add(p);
            db.SaveChanges();
            return RedirectToAction("List");
        }

        public IActionResult Delete(int? id)
        {
            // 1. 建立資料庫連線
            PetDbContext db = new PetDbContext();

            // 2. 找到那隻寵物
            Pet x = db.Pets.FirstOrDefault(p => p.PetId == id);

            if (x != null)
            {
                // ❌ 原本的硬刪除寫法： db.Pets.Remove(x);

                // ✅ 改成軟刪除：給它一個刪除時間
                x.DeletedAt = DateTime.Now;

                // 儲存變更
                db.SaveChanges();
            }

            return RedirectToAction("List");
        }

        public IActionResult Edit(int? id)
        {
            PetDbContext db = new PetDbContext();
            Pet x = db.Pets.FirstOrDefault(p => p.PetId == id);
            if (x == null)
                return RedirectToAction("List");
            return View(x);
        }
        [HttpPost]
        public IActionResult Edit(Pet uiPet) // 變數名稱改叫 uiPet 比較不會搞混
        {
            PetDbContext db = new PetDbContext();

            // 1. 根據使用者傳回來的 PetId，從資料庫把「舊的那筆資料」抓出來
            Pet dbPet = db.Pets.FirstOrDefault(p => p.PetId == uiPet.PetId);

            // 2. 確保資料庫真的有這筆資料！(這一步很重要)
            if (dbPet != null)
            {
                // 3. 將表單傳進來的新資料 (uiPet)，覆蓋掉資料庫裡的舊資料 (dbPet)
                // (這裡我列出了你第一張截圖裡的寵物屬性，你可以把你不想被修改的欄位刪除)
                dbPet.SpeciesId = uiPet.SpeciesId;
                dbPet.Name = uiPet.Name;
                dbPet.Gender = uiPet.Gender;
                dbPet.Size = uiPet.Size;
                dbPet.CoatColor = uiPet.CoatColor;
                dbPet.CurrentStatus = uiPet.CurrentStatus;
                dbPet.BehavioralTraits = uiPet.BehavioralTraits;
                dbPet.IsHighMaintenance = uiPet.IsHighMaintenance;
                dbPet.Note = uiPet.Note;
                dbPet.IsDesex = uiPet.IsDesex;

                // ✨ 小細節：如果你的資料表有 UpdatedAt 欄位，這時候剛好可以寫入修改時間
                dbPet.UpdatedAt = DateTime.Now;

                // 4. 告訴資料庫把變更存起來
                db.SaveChanges();
            }

            // 修改完成後，回到列表頁看結果
            return RedirectToAction("List");
        }
    }
}
