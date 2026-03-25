using Microsoft.AspNetCore.Mvc;
using PawsPort.Models;

namespace PawsPort.Controllers
{
    public class ArticleController : Controller
    {
        //public IActionResult Index()
        //{
        //    return View();
        //}

        public IActionResult List()
        {
            PetdbContext db = new PetdbContext(); //建立資料庫上下文
            string keyword = null; //這裡可以替換成實際的搜尋關鍵字
            List<Article> datas = null;
            if (string.IsNullOrEmpty(keyword))
            {
                datas = db.Articles.Where(a => a.IsExist).ToList(); //查詢所有存在的文章

            }
            else
            {
                string searchTerm = null; //這裡可以替換成實際的搜尋條件
                datas = db.Articles.Where(a => a.IsExist && (a.Title.Contains(searchTerm) || a.Content.Contains(searchTerm))).ToList(); //根據搜尋條件查詢文章

            }

            return View();
        }
    }
}
