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

            // 步驟 1：先寫好基礎查詢 (把兩張表 Join 起來，並轉換成 ViewModel)
            // 注意：這時候還沒有真正去資料庫抓資料喔，只是先寫好「查詢計畫」
            var query = from h in db.HealthPassports
                        join p in db.Pets on h.PetId equals p.PetId
                        // 過濾掉已經被軟刪除的寵物 (確保資料是乾淨的)
                        where p.DeletedAt == null
                        select new HealthPassportListViewModel
                        {
                            PassportId = h.PassportId,
                            PetId = h.PetId,
                            Name = p.Name, // 順利拿到名字
                            Weight = h.Weight,
                            Note = h.Note,
                            RecordType = h.RecordType,
                            RecordDate = h.RecordDate,
                            UpdatedAt = h.UpdatedAt,
                            CreatedAt = h.CreatedAt
                        };
            

            // 步驟 2：加上你的「搜尋功能」邏輯！
            if (!string.IsNullOrEmpty(vm.txtKeyword))
            {
                // 輸入關鍵字，就在剛才組好的 ViewModel 裡面，尋找符合「寵物名字」的資料
                query = query.Where(v => v.Name.Contains(vm.txtKeyword));

                // 搜尋名字，也能搜尋護照的備註 (Note)，可以改成這樣寫：
                // query = query.Where(v => v.PetName.Contains(vm.txtKeyword) || (v.Note != null && v.Note.Contains(vm.txtKeyword)));
            }

            // 步驟 3：執行查詢，把資料變成 List，然後傳給 View
            return View(query.ToList());
        }

        public IActionResult Create()
        {
            return View();
        }
        [HttpPost]
        public IActionResult Create(HealthPassport p)
        {
            PetDbContext db = new PetDbContext();
            p.PassportId = 0;
            db.HealthPassports.Add(p);
            db.SaveChanges();
            return RedirectToAction("List");
        }

        public IActionResult Delete(int? id)
        {
            // 1. 建立資料庫連線
            PetDbContext db = new PetDbContext();

            // 2. 找到那筆健康資料
            HealthPassport x = db.HealthPassports.FirstOrDefault(p => p.PassportId == id);

            if (x != null)
            {
                // 這裡到時候要改成軟刪除
                db.HealthPassports.Remove(x);
                db.SaveChanges();
            }

            return RedirectToAction("List");
        }

        public IActionResult Edit(int? id)
        {
            PetDbContext db = new PetDbContext();
            HealthPassport x = db.HealthPassports.FirstOrDefault(p => p.PassportId == id);
            if (x == null)
                return RedirectToAction("List");
            return View(x);
        }
        [HttpPost]
        public IActionResult Edit(HealthPassport uiPassport) // 變數名稱改叫 uiPassport 比較不會搞混
        {
            PetDbContext db = new PetDbContext();

            // 1. 根據使用者傳回來的 PassportId，從資料庫把「舊的那筆資料」抓出來
            HealthPassport dbPassport = db.HealthPassports.FirstOrDefault(p => p.PassportId == uiPassport.PassportId);

            // 2. 確保資料庫真的有這筆資料！(這一步很重要)
            if (dbPassport != null)
            {
                // 3. 將表單傳進來的新資料 (uiPassport)，覆蓋掉資料庫裡的舊資料 (dbPassport)
                // (這裡我列出了你第一張截圖裡的寵物屬性，你可以把你不想被修改的欄位刪除)
                dbPassport.Weight = uiPassport.Weight;
                dbPassport.Note = uiPassport.Note;
                dbPassport.RecordType = uiPassport.RecordType;


                // ✨ 小細節：如果你的資料表有 UpdatedAt 欄位，這時候剛好可以寫入修改時間
                dbPassport.UpdatedAt = DateTime.Now;

                // 4. 告訴資料庫把變更存起來
                db.SaveChanges();
            }

            // 修改完成後，回到列表頁看結果
            return RedirectToAction("List");
        }
    }
}
