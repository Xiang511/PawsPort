using Microsoft.AspNetCore.Mvc;
using PawsPort.Models;

namespace PawsPort.Controllers
{
    public class SupportController : Controller
    {
        //FAQ
              

        public ActionResult List()
        {
            PetDbContext db = new PetDbContext();
            var faq = db.Faqs.Where(f => f.IsExist == true).ToList();
            return View(faq);
        }

        public ActionResult Create()
        {
            return View();
        }

        [HttpPost]
        public ActionResult Create(Faq f)
        {

            ModelState.Remove("Status");
            ModelState.Remove("CreateAt");
            ModelState.Remove("IsExist");

            if (ModelState.IsValid)
            {
                PetDbContext db = new PetDbContext();

                f.CreateAt = DateTime.Now;
                f.IsExist = true;
                f.Status = "待發佈";

                if (string.IsNullOrEmpty(f.Note))
                {
                    f.Note = "無";
                }


                db.Faqs.Add(f);
                db.SaveChanges();
                return RedirectToAction("List");
            }
            return View(f);

        }




        public ActionResult Edit(int id)
        {
            PetDbContext db = new PetDbContext();

            // 根據傳進來的id，去資料庫找對應的資料
            var faq = db.Faqs.FirstOrDefault(f => f.Faqid == id);

            if (faq == null)
            {
                return RedirectToAction("List");
            }


            return View(faq);
        }


        [HttpPost]
        public ActionResult Edit(Faq f)
        {
                        
            ModelState.Remove("CreateAt");
            ModelState.Remove("IsExist");

            if (ModelState.IsValid)
            {
                PetDbContext db = new PetDbContext();

                // 從資料庫把舊資料抓出來
                var Faqdata = db.Faqs.FirstOrDefault(x => x.Faqid == f.Faqid);

                if (Faqdata != null)
                {
                    // 將新資料蓋過去舊資料
                    Faqdata.QuestionType = f.QuestionType;
                    Faqdata.Question = f.Question;
                    Faqdata.Answer = f.Answer;
                    Faqdata.Status = f.Status;

                    // 備註防呆處理
                    if (string.IsNullOrEmpty(f.Note))
                    {
                        Faqdata.Note = "無";
                    }
                    else
                    {
                        Faqdata.Note = f.Note;
                    }


                    db.SaveChanges();
                    return RedirectToAction("List");
                }
            }


            return View(f);
        }



        public ActionResult Delete(int id)
        {
           
            PetDbContext db = new PetDbContext();
                        
            var faqItem = db.Faqs.FirstOrDefault(f => f.Faqid == id);

            
            if (faqItem != null)
            {
                //db.Faqs.Remove(faqItem);
                faqItem.IsExist = false;               
                faqItem.StoppedDate = DateTime.Now;    
                faqItem.Status = "已下架";

                db.SaveChanges();
            }

            
            return RedirectToAction("List");
        }






        //QARecord


        public ActionResult QaList()
        {
            PetDbContext db = new PetDbContext();
            var qaList = db.QARecords.ToList();
            return View(qaList);
        }



        
        public ActionResult QaEdit(int id)
        {
            PetDbContext db = new PetDbContext();
                    
            var qa = db.QARecords.FirstOrDefault(q => q.Qaid == id);

            if (qa == null)
            {
                return RedirectToAction("QaList");
            }

            return View(qa);
        }

    
        [HttpPost]
        public ActionResult QaEdit(QARecord q)
        {
            PetDbContext db = new PetDbContext();
                 
            var Qadata = db.QARecords.FirstOrDefault(x => x.Qaid == q.Qaid);

            if (Qadata != null)
            {
                Qadata.Csname = q.Csname;
                Qadata.ReplyContent = q.ReplyContent;
                Qadata.Note = q.Note;

                
                Qadata.ReplyDate = DateTime.Now;

                db.SaveChanges();
                return RedirectToAction("QaList");
            }

            return RedirectToAction("QaList");
        }



        public ActionResult QaDetails(int id)
        {
            PetDbContext db = new PetDbContext();

            
            var qa = db.QARecords.FirstOrDefault(q => q.Qaid == id);

            if (qa == null)
            {
                return RedirectToAction("QaList");
            }

            return View(qa);
        }
    }
}
