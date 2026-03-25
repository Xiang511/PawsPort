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

        public IActionResult CategoryList(KeywordViewModel vm)
        {
            PetDbContext db = new PetDbContext();

            IEnumerable<Category> datas = null; //宣告一個變數來存放查詢結果

            if (string.IsNullOrEmpty(vm.txtCategoryId.ToString()))
            {
               
                datas = db.Categories.Where(a => a.IsExist).ToList(); //查詢所有存在的類別
            }
            else
            {
                datas = db.Categories.Where(a => a.IsExist
                && (a.CategoryName.Contains(vm.txtKeyword)
                )).ToList(); //根據搜尋條件查詢類別
            }

            return View(datas);
        }


        
        public IActionResult ArticleList(KeywordViewModel vm) //貼文管理頁面
        {
            PetDbContext db = new PetDbContext();

            IEnumerable<Article> datas = null; //宣告一個變數來存放查詢結果
            if (string.IsNullOrEmpty(vm.txtKeyword))
            {
                datas = from p in db.Articles
                        where p.IsExist
                        select p;
            }
            else
            {
                datas = db.Articles.Where(p => p.IsExist 
                && (p.Title.Contains(vm.txtKeyword)
                || p.Content.Contains(vm.txtKeyword))); //根據搜尋條件查詢文章
            }

            return View(datas);
        }


        public IActionResult CreateArticle()
        {
            return View();
        }


        [HttpPost]
        public IActionResult CreateArticle(Article p)
        {
            PetDbContext db = new PetDbContext();

            p.IsExist = true; //設定文章為存在狀態
            db.Articles.Add(p);
            db.SaveChanges();
            return RedirectToAction("ArticleList");
          
        }




        public IActionResult ArticleImageList(KeywordViewModel vm)
        {
            PetDbContext db = new PetDbContext();

            IEnumerable<ArticleImage> datas = null; //宣告一個變數來存放查詢結果
            if (string.IsNullOrEmpty(vm.txtArticleId.ToString()))
            {
                datas = db.ArticleImages.Where(a => a.IsExist).ToList(); //查詢所有存在的文章圖片
            }
            else
            {
                datas = db.ArticleImages.Where(a => a.IsExist 
                && (a.ArticleId == vm.txtArticleId
                )).ToList(); //根據搜尋條件查詢文章圖片
            }
            return View(datas);
        }



        public IActionResult EventList(KeywordViewModel vm) //活動管理頁面
        {
            PetDbContext db = new PetDbContext();
            vm.txtCategoryId = 1; //假設活動的CategoryId為1

            IEnumerable<Article> datas = null; //宣告一個變數來存放查詢結果

            datas = db.Articles.Where(a => a.IsExist
            && (a.CategoryId == vm.txtCategoryId) //篩選出活動類別的文章
            || (a.Title.Contains(vm.txtKeyword)
            || a.Content.Contains(vm.txtKeyword))
            || a.EventLocation.Contains(vm.txtKeyword)
                 ); //根據搜尋條件查詢文章

            return View(datas);

        }

    }
}
