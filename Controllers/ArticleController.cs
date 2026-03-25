using Microsoft.AspNetCore.Mvc;
using PawsPort.Models;
using PawsPort.ViewModels;

namespace PawsPort.Controllers
{
    public class ArticleController : Controller
    {
        //public IActionResult Index()
        //{
        //    return View();
        //}

        public IActionResult List(KeywordViewModel vm)
        {
            PetDbContext db = new PetDbContext();

            IEnumerable<Article> datas = null; //宣告一個變數來存放查詢結果
            if (string.IsNullOrEmpty(vm.txtKeyword))
            {
                datas = db.Articles.Where(a => a.IsExist).ToList(); //查詢所有存在的文章
            }
            else
            {
                datas = db.Articles.Where(a => a.IsExist && (a.Title.Contains(vm.txtKeyword) || a.Content.Contains(vm.txtKeyword))).ToList(); //根據搜尋條件查詢文章
            }

            return View(datas);
        }
    }
}
