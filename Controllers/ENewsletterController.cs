using Microsoft.AspNetCore.Mvc;
using Microsoft.CodeAnalysis.Options;
using PawsPort.Models;
using System.Linq;

namespace PawsPort.Controllers
{
    public class ENewsletterController : Controller
    {
        public ActionResult List()
        {
            PetDbContext db = new PetDbContext();

            
            var categoryOrder = new List<string>
            {
                  "活動公告",
                  "認養資訊",
                  "飼養知識",
                  "遊戲挑戰",
                  "其它類別"
                  
              };
            var listFromDb = db.ENewsletters
                       .Where(n => n.Status != "已刪除")
                       .ToList();

            var list = listFromDb
                      .OrderBy(n =>
                      {
                          // 去問：這個分類排在名單裡的第幾個位置？ (0, 1, 2, 3, 4)
                          int index = categoryOrder.IndexOf(n.Category);
                          // 防呆：如果以前有舊資料的分類不在名單上 (index 會是 -1)，就把它踢到最後面 (給它 99 號)
                          return index == -1 ? 99 : index;
                      })

                      .ThenByDescending(n => n.NewsLetterId) // 同一個分類裡，最新的依然排最上面
                      .ToList();

            return View(list);
        }




        public ActionResult Create()
        {
            return View();
        }


        [HttpPost]
        public ActionResult Create(ENewsletter n)
        {

            if (string.IsNullOrEmpty(n.Status))
            {
                n.Status = "草稿";
            }


            if (n.Status == "已發送" && n.PublishDate == null)
            {
                n.PublishDate = DateTime.Now;
            }

            // 實務上這裡會抓取「目前登入者的會員ID」，但在我們接上登入功能前，先預設給 1 避免資料庫報錯
            if (n.UserId == 0)
            {
                n.UserId = 1;
            }


            PetDbContext db = new PetDbContext();
            db.ENewsletters.Add(n);
            db.SaveChanges();


            return RedirectToAction("List");
        }



        public ActionResult Edit(int? id)
        {
            if (id == null) return RedirectToAction("List");

            PetDbContext db = new PetDbContext();

            var news = db.ENewsletters.FirstOrDefault(n => n.NewsLetterId == id);

            if (news == null) return RedirectToAction("List");

            return View(news);
        }


        [HttpPost]
        public ActionResult Edit(ENewsletter n)
        {
            PetDbContext db = new PetDbContext();


            var newsInDb = db.ENewsletters.FirstOrDefault(x => x.NewsLetterId == n.NewsLetterId);

            if (newsInDb != null)
            {
                newsInDb.Title = n.Title;
                newsInDb.Summary = n.Summary;
                newsInDb.Content = n.Content;
                newsInDb.Category = n.Category;
                newsInDb.Status = string.IsNullOrEmpty(n.Status) ? "草稿" : n.Status;
                newsInDb.Note = n.Note;


                if (newsInDb.Status == "已發送" && n.PublishDate == null)
                {
                    newsInDb.PublishDate = DateTime.Now;
                }
                else
                {
                    newsInDb.PublishDate = n.PublishDate;
                }


                db.SaveChanges();
            }

            return RedirectToAction("List");
        }



        public ActionResult Delete(int? id)
        {
            if (id == null) return RedirectToAction("List");

            PetDbContext db = new PetDbContext();
            var news = db.ENewsletters.FirstOrDefault(n => n.NewsLetterId == id);

            if (news != null)
            {
                news.Status = "已刪除";

                db.SaveChanges();
            }

            return RedirectToAction("List");
        }
    }
}
