using Microsoft.AspNetCore.Mvc;
using PawsPort.Models;
using PawsPort.ViewModels;

namespace PawsPort.Controllers
{
    public class AdoptionRecordController : Controller
    {
        public IActionResult List(KeywordViewModel vm)
        {
            PetDbContext db = new PetDbContext();


            var query = from h in db.AdoptionRecords
                        join p in db.Pets on h.PetId equals p.PetId

                        where p.DeletedAt == null
                        select new AdoptionRecordViewModel
                        {
                            AdoptionId = h.AdoptionId,
                            PetId = h.PetId,
                            Name = p.Name, // 順利拿到名字
                            UserId = h.UserId,
                            ApplyDate = h.ApplyDate,
                            AdoptDate = h.AdoptDate,
                            ReturnDate = h.ReturnDate,
                            ReturnReason = h.ReturnReason,
                            FollowUpDeadline = h.FollowUpDeadline,
                            Status = h.Status
                            
                        };


            if (!string.IsNullOrEmpty(vm.txtKeyword))
            {

                query = query.Where(v => v.Name.Contains(vm.txtKeyword));


            }

            // 步驟 3：執行查詢，把資料變成 List，然後傳給 View
            return View(query.ToList());
        }

        public IActionResult Create()
        {
            return View();
        }
        [HttpPost]
        public IActionResult Create(AdoptionRecord p)
        {
            PetDbContext db = new PetDbContext();
            p.AdoptionId = 0;
            db.AdoptionRecords.Add(p);
            db.SaveChanges();
            return RedirectToAction("List");
        }

        public IActionResult Delete(int? id)
        {
            // 1. 建立資料庫連線
            PetDbContext db = new PetDbContext();

            // 2. 找到那筆健康資料
            AdoptionRecord x = db.AdoptionRecords.FirstOrDefault(p => p.AdoptionId == id);

            if (x != null)
            {
                // 這裡到時候要改成軟刪除
                db.AdoptionRecords.Remove(x);
                db.SaveChanges();
            }

            return RedirectToAction("List");
        }

        public IActionResult Edit(int? id)
        {
            PetDbContext db = new PetDbContext();
            AdoptionRecord x = db.AdoptionRecords.FirstOrDefault(p => p.AdoptionId == id);
            if (x == null)
                return RedirectToAction("List");
            return View(x);
        }
        [HttpPost]
        public IActionResult Edit(AdoptionRecord uiAdoption) // 變數名稱改叫 uiPassport 比較不會搞混
        {
            PetDbContext db = new PetDbContext();

            // 1. 根據使用者傳回來的 PassportId，從資料庫把「舊的那筆資料」抓出來
            AdoptionRecord dbAdoption = db.AdoptionRecords.FirstOrDefault(p => p.AdoptionId == uiAdoption.AdoptionId);

            // 2. 確保資料庫真的有這筆資料！(這一步很重要)
            if (dbAdoption != null)
            {
                // 3. 將表單傳進來的新資料 (uiPassport)，覆蓋掉資料庫裡的舊資料 (dbPassport)
                // (這裡我列出了你第一張截圖裡的寵物屬性，你可以把你不想被修改的欄位刪除)
                dbAdoption.ApplyDate = uiAdoption.ApplyDate;
                dbAdoption.AdoptDate = uiAdoption.AdoptDate;
                dbAdoption.ReturnDate = uiAdoption.ReturnDate;
                dbAdoption.ReturnReason = uiAdoption.ReturnReason;
                dbAdoption.FollowUpDeadline = uiAdoption.FollowUpDeadline;
                dbAdoption.UserId = uiAdoption.UserId;
                dbAdoption.Status = uiAdoption.Status;




                // 4. 告訴資料庫把變更存起來
                db.SaveChanges();
            }

            // 修改完成後，回到列表頁看結果
            return RedirectToAction("List");
        }
    }
}
