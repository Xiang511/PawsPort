using Microsoft.AspNetCore.Mvc;
using PawsPort.Dtos;
using PawsPort.Responses;
using PawsPort.Services;
using Serilog;

namespace PawsPort.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Produces("application/json")]


    public class SupportController : ApiControllerBase
    {

        private readonly FaqService _faqService;
        private readonly QaService _qaService;

        public SupportController(FaqService faqService, QaService qaService)
        {
            _faqService = faqService;
            _qaService = qaService;
        }

        //List
        [HttpGet("Faq")]
        [ProducesResponseType(typeof(ApiResponse<List<FaqDTO>>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status204NoContent)]

        public async Task<IActionResult> GetFaqs()
        {
            var faqs = await _faqService.GetAllFaqsAsync();

            if (faqs == null || !faqs.Any())
            {
                return NoContent();
            }

            return Success(faqs, "成功取得FAQ列表", 200);
        }


        [HttpPost("Faq")]
        [ProducesResponseType(typeof(ApiResponse<FaqDTO>), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]

        public async Task<IActionResult> CreateFaq(FaqCreateDTO dto)
        {
            if (!ModelState.IsValid)
            {
                return Failure("VALIDATION_ERROR", "資料驗證失敗，請檢查必填欄位", 400);
            }

            var result = await _faqService.CreateFaqAsync(dto);

            Log.Information("新增FAQ成功 FAQ_ID:{Faqid}", result.Faqid);

            return Success(result, "創建FAQ成功", 201);
        }


        [HttpPut("Faq/{id}")]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]

        public async Task<IActionResult> UpdateFaq(int id, FaqUpdateDTO dto)
        {
            if (!ModelState.IsValid)
            {
                return Failure("VALIDATION_ERROR", "資料驗證失敗，請檢查欄位", 400);
            }

            var isSuccess = await _faqService.UpdateFaqAsync(id, dto);

            if (!isSuccess)
            {
                return Failure("FAQ_NOT_FOUND", "找不到指定的FAQ，更新失敗", 404);
            }

            return Success<object>(null, "更新FAQ成功", 200);
        }


        [HttpDelete("Faq/{id}")]


        public async Task<IActionResult> DeleteFaq(string id)
        {
            if (string.IsNullOrWhiteSpace(id))
            {
                return Failure("FAQ_ID_EMPTY", "FAQ ID不能為空", 400);
            }

            if (!int.TryParse(id, out int faqId))
            {
                return Failure("FAQ_ID_INVALID", "FAQ ID格式錯誤，必須是數字", 400);
            }

            var result = await _faqService.DeleteFaqAsync(faqId);

            if (!result)
            {
                return Failure("FAQ_NOT_FOUND", "找不到指定的FAQ", 404);
            }

            return NoContent();
        }




        [HttpGet("Qa")]
        public async Task<IActionResult> GetQaList()
        {
            var result = await _qaService.GetAllQaAsync();
            return Success(result, "取得QA列表成功", 200);
        }


        //取得單筆QA明細
        [HttpGet("Qa/{id}")]
        public async Task<IActionResult> GetQaDetails(int id)
        {
            var result = await _qaService.GetQaByIdAsync(id);
            if (result == null)
            {
                return Failure("QA_NOT_FOUND", "找不到指定的問答紀錄", 404);
            }
            return Success(result, "取得QA明細成功", 200);
        }


        [HttpPut("Qa/{id}")]
        public async Task<IActionResult> UpdateQa(int id, QaUpdateDTO dto)
        {
            if (!ModelState.IsValid)
            {
                return Failure("VALIDATION_ERROR", "資料格式錯誤", 400);
            }

            var isSuccess = await _qaService.UpdateQaAsync(id, dto);
            if (!isSuccess)
            {
                return Failure("QA_NOT_FOUND", "找不到該筆問答紀錄，更新失敗", 404);
            }

            return Success<object>(null, "QA回覆成功", 200);
        }
    }
}

//public class SupportController : Controller
//{
//    //FAQ


//    public ActionResult List()
//    {
//        PetDbContext db = new PetDbContext();
//        var faq = db.Faqs.Where(f => f.IsExist == true).ToList();
//        return View(faq);
//    }

//    public ActionResult Create()
//    {
//        return View();
//    }

//    [HttpPost]
//    public ActionResult Create(Faq f)
//    {

//        ModelState.Remove("Status");
//        ModelState.Remove("CreateAt");
//        ModelState.Remove("IsExist");

//        if (ModelState.IsValid)
//        {
//            PetDbContext db = new PetDbContext();

//            f.CreateAt = DateTime.Now;
//            f.IsExist = true;
//            f.Status = "待發佈";

//            if (string.IsNullOrEmpty(f.Note))
//            {
//                f.Note = "無";
//            }


//            db.Faqs.Add(f);
//            db.SaveChanges();
//            return RedirectToAction("List");
//        }
//        return View(f);

//    }




//        public ActionResult Edit(int id)
//        {
//            PetDbContext db = new PetDbContext();

//            // 根據傳進來的id，去資料庫找對應的資料
//            var faq = db.Faqs.FirstOrDefault(f => f.Faqid == id);

//            if (faq == null)
//            {
//                return RedirectToAction("List");
//            }


//            return View(faq);
//        }


//        [HttpPost]
//        public ActionResult Edit(Faq f)
//        {

//            ModelState.Remove("CreateAt");
//            ModelState.Remove("IsExist");

//            if (ModelState.IsValid)
//            {
//                PetDbContext db = new PetDbContext();

//                // 從資料庫把舊資料抓出來
//                var Faqdata = db.Faqs.FirstOrDefault(x => x.Faqid == f.Faqid);

//                if (Faqdata != null)
//                {
//                    // 將新資料蓋過去舊資料
//                    Faqdata.QuestionType = f.QuestionType;
//                    Faqdata.Question = f.Question;
//                    Faqdata.Answer = f.Answer;
//                    Faqdata.Status = f.Status;

//                    // 備註防呆處理
//                    if (string.IsNullOrEmpty(f.Note))
//                    {
//                        Faqdata.Note = "無";
//                    }
//                    else
//                    {
//                        Faqdata.Note = f.Note;
//                    }


//                    db.SaveChanges();
//                    return RedirectToAction("List");
//                }
//            }


//            return View(f);
//        }



//        public ActionResult Delete(int id)
//        {

//            PetDbContext db = new PetDbContext();

//            var faqItem = db.Faqs.FirstOrDefault(f => f.Faqid == id);


//            if (faqItem != null)
//            {
//                //db.Faqs.Remove(faqItem);
//                faqItem.IsExist = false;               
//                faqItem.StoppedDate = DateTime.Now;    
//                faqItem.Status = "已下架";

//                db.SaveChanges();
//            }


//            return RedirectToAction("List");
//        }






//        //QARecord


//        public ActionResult QaList()
//        {
//            PetDbContext db = new PetDbContext();
//            var qaList = db.QARecords.ToList();
//            return View(qaList);
//        }




//        public ActionResult QaEdit(int id)
//        {
//            PetDbContext db = new PetDbContext();

//            var qa = db.QARecords.FirstOrDefault(q => q.Qaid == id);

//            if (qa == null)
//            {
//                return RedirectToAction("QaList");
//            }

//            return View(qa);
//        }


//        [HttpPost]
//        public ActionResult QaEdit(QARecord q)
//        {
//            PetDbContext db = new PetDbContext();

//            var Qadata = db.QARecords.FirstOrDefault(x => x.Qaid == q.Qaid);

//            if (Qadata != null)
//            {
//                Qadata.Csname = q.Csname;
//                Qadata.ReplyContent = q.ReplyContent;
//                Qadata.Note = q.Note;


//                Qadata.ReplyDate = DateTime.Now;

//                db.SaveChanges();
//                return RedirectToAction("QaList");
//            }

//            return RedirectToAction("QaList");
//        }



//        public ActionResult QaDetails(int id)
//        {
//            PetDbContext db = new PetDbContext();


//            var qa = db.QARecords.FirstOrDefault(q => q.Qaid == id);

//            if (qa == null)
//            {
//                return RedirectToAction("QaList");
//            }

//            return View(qa);
//        }
//    }
//}
