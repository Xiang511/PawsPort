using Microsoft.AspNetCore.Mvc;
using PawsPort.Models;
using System.Linq;

namespace PawsPort.Controllers
{
    public class ENewsletterController : Controller
    {
        public ActionResult List()
        {
            PetDbContext db = new PetDbContext();

            var enewsList =db.ENewsletters.OrderByDescending(n => n.NewsLetterId).ToList();

            return View(enewsList);
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
    }
}
