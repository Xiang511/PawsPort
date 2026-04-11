using Microsoft.AspNetCore.Mvc;
using PawsPort.Models;
using PawsPort.ViewModels;

namespace PawsPort.Controllers
{
    public class MissingReportsController : Controller
    {
        public IActionResult List(KeywordViewModel vm)
        {
            PetDbContext db = new PetDbContext();

           
            var query = from h in db.MissingReports
                        join p in db.Pets on h.PetId equals p.PetId
                        
                        where p.DeletedAt == null
                        select new MissingReportListViewModel
                        {
                            ReportId = h.ReportId,
                            PetId = h.PetId,
                            Name = p.Name, // 順利拿到名字
                            LastSeenDate = h.LastSeenDate,
                            IsActive = h.IsActive,
                            LastSeenLat = h.LastSeenLat,
                            LastSeenLng = h.LastSeenLng,
                            LostLocation = h.LostLocation,
                            UpdatedAt = h.UpdatedAt,
                            CreatedAt = h.CreatedAt,
                            UserId = h.UserId
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
        public IActionResult Create(MissingReport p)
        {
            PetDbContext db = new PetDbContext();
            p.ReportId = 0;
            db.MissingReports.Add(p);
            db.SaveChanges();
            return RedirectToAction("List");
        }

        public IActionResult Delete(int? id)
        {
            // 1. 建立資料庫連線
            PetDbContext db = new PetDbContext();

            // 2. 找到那筆健康資料
            MissingReport x = db.MissingReports.FirstOrDefault(p => p.ReportId == id);

            if (x != null)
            {
                // 這裡到時候要改成軟刪除
                db.MissingReports.Remove(x);
                db.SaveChanges();
            }

            return RedirectToAction("List");
        }

        public IActionResult Edit(int? id)
        {
            PetDbContext db = new PetDbContext();
            MissingReport x = db.MissingReports.FirstOrDefault(p => p.ReportId == id);
            if (x == null)
                return RedirectToAction("List");
            return View(x);
        }
        [HttpPost]
        public IActionResult Edit(MissingReport uiReport) // 變數名稱改叫 uiPassport 比較不會搞混
        {
            PetDbContext db = new PetDbContext();

            // 1. 根據使用者傳回來的 PassportId，從資料庫把「舊的那筆資料」抓出來
            MissingReport dbReport = db.MissingReports.FirstOrDefault(p => p.ReportId == uiReport.ReportId);

            // 2. 確保資料庫真的有這筆資料！(這一步很重要)
            if (dbReport != null)
            {
                // 3. 將表單傳進來的新資料 (uiPassport)，覆蓋掉資料庫裡的舊資料 (dbPassport)
                // (這裡我列出了你第一張截圖裡的寵物屬性，你可以把你不想被修改的欄位刪除)
                dbReport.LastSeenDate = uiReport.LastSeenDate;
                dbReport.LastSeenLat = uiReport.LastSeenLat;
                dbReport.LastSeenLng = uiReport.LastSeenLng;
                dbReport.IsActive = uiReport.IsActive;
                dbReport.LostLocation = uiReport.LostLocation;


                
                dbReport.UpdatedAt = DateTime.Now;

                // 4. 告訴資料庫把變更存起來
                db.SaveChanges();
            }

            // 修改完成後，回到列表頁看結果
            return RedirectToAction("List");
        }
    }
}
