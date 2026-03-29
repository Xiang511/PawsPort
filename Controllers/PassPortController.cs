
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

        //public IActionResult Create()
        //{
        //    return View();
        //}
        //[HttpPost]
        //public IActionResult Create(HealthPassport p)
        //{
        //    PetDbContext db = new PetDbContext();
        //    p.PassportId = 0;
        //    db.HealthPassports.Add(p);
        //    db.SaveChanges();
        //    return RedirectToAction("List");
        //}

        public IActionResult Create()
        {
            return View();
        }
        [HttpPost]
        [HttpPost]
        public IActionResult Create(HealthPassportCreateViewModel vm)
        {
            PetDbContext db = new PetDbContext();

            // ==========================================
            // 步驟 1：拆解包裹，先新增「主表」(健康護照)
            // ==========================================
            // 創建一個真實對應資料庫的 Model 實體
            HealthPassport passport = new HealthPassport
            {
                PetId = vm.PetId,
                Weight = vm.Weight,
                Note = vm.Note,
                RecordType = vm.RecordType,
                RecordDate = vm.RecordDate,
                CreatedAt = DateTime.Now // 實務上，建立時間通常在程式裡給定當下時間
            };

            // 把護照加入資料庫並「馬上存檔」！
            db.HealthPassports.Add(passport);
            db.SaveChanges();
            // 💡 魔法發生了：SaveChanges() 執行完後，資料庫會自動生成一個新的流水號。
            // 這時候 passport.PassportId 就已經不是 0 了，而是資料庫給的最新 ID！


            // ==========================================
            // 步驟 2：判斷並新增「病歷」資料 (MedicalHistory)
            // ==========================================
            // 怎麼知道使用者有沒有填病歷？我們檢查必填欄位 (例如 Disease) 是不是有值
            if (!string.IsNullOrEmpty(vm.Disease))
            {
                MedicalHistory medical = new MedicalHistory
                {
                    // 🔑 關鍵！把剛剛產生的新護照 ID 綁定給病歷
                    PassportId = passport.PassportId,

                    Location = vm.TreatmentLocation,
                    Disease = vm.Disease,
                    DiseaseTreatment = vm.DiseaseTreatment,
                    Time = vm.TreatmentTime,
                    CreatedAt = DateTime.Now
                };
                db.MedicalHistories.Add(medical);
            }


            // ==========================================
            // 步驟 3：判斷並新增「疫苗」資料 (Vaccination)
            // ==========================================
            // 一樣，檢查疫苗的必填欄位 (例如 Type) 是不是有值
            if (!string.IsNullOrEmpty(vm.Type))
            {
                VaccinationStatus vaccine = new VaccinationStatus
                {
                    // 🔑 關鍵！把剛剛產生的新護照 ID 綁定給疫苗
                    PassportId = passport.PassportId,

                    Type = vm.Type,
                    Location = vm.VaccinationLocation,
                    Time = vm.VaccinationTime,
                    Forecast = vm.Forecast,
                    CreatedAt = DateTime.Now
                };
                db.VaccinationStatuses.Add(vaccine);
            }

            // ==========================================
            // 步驟 4：把剛才加入的病歷和疫苗，正式存進資料庫
            // ==========================================
            // 如果上面沒有填寫病歷或疫苗，這行依然可以安全執行，不會報錯
            db.SaveChanges();

            // 成功後，帶點好消息回到列表頁！
            TempData["SuccessMessage"] = "新增資料成功！";
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

        public IActionResult Details(int? id)
        {
            PetDbContext db = new PetDbContext();
            if (id == null) return RedirectToAction("List");

            // 1. 分開查詢：去病歷表找，找不到的話 m 就會是 null
            var m = db.MedicalHistories.FirstOrDefault(x => x.PassportId == id);

            // 2. 分開查詢：去疫苗表找，找不到的話 v 就會是 null
            var v = db.VaccinationStatuses.FirstOrDefault(x => x.PassportId == id);

            // 3. 防呆機制：如果「病歷」和「疫苗」都完全沒資料，才退回列表
            if (m == null && v == null)
            {
                TempData["ErrorMessage"] = "該名寵物目前沒有任何病歷與疫苗資料喔！";
                return RedirectToAction("List");
            }

            // 4. 組裝 ViewModel (使用 ? 和 ?? 魔法)
            HealthPassportDetailsViewModel pet = new HealthPassportDetailsViewModel
            {
                PassportId = (int)id,

                // --- 病歷資料 ---
                // m?.MedicalDetailId 意思是：如果 m 不是空的，就給我 ID；如果是空的，就給 null，不要報錯！
                MedicalDetailId = m?.MedicalDetailId,

                // m?.Location ?? "尚未有資料" 意思是：給我 Location，如果沒有，就顯示"尚未有資料"！
                TreatmentLocation = m?.Location ?? "尚未有資料",
                Disease = m?.Disease ?? "尚未有資料",
                DiseaseTreatment = m?.DiseaseTreatment ?? "尚未有資料",
                TreatmentTime = m?.Time, // 時間欄位先保持原樣

                // --- 疫苗資料 ---
                HistoryId = v?.HistoryId,
                Type = v?.Type ?? "尚未有資料",
                VaccinationLocation = v?.Location ?? "尚未有資料",
                VaccinationTime = v?.Time
            };

            return View(pet);
        }
    }
}
