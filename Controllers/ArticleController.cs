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



        public IActionResult ArticleList(KeywordViewModel vm) //貼文管理頁面
        {

            using (PetDbContext db = new PetDbContext()) //查詢完畢後就關閉
            {
                var query = db.Articles.Where(p => p.IsExist); //查詢所有存在的文章

                if (!string.IsNullOrEmpty(vm.txtKeyword))
                {
                    query = query.Where(p => p.Title.Contains(vm.txtKeyword)
                  || p.Content.Contains(vm.txtKeyword)); //根據搜尋條件查詢文章
                }

                var datas = query.ToList().Select(p => new ArticleWrap { article = p }); //將查詢結果轉換為ArticleWrap物件的列表

                return View(datas);
            }

        }


        public IActionResult CreateArticle()
        {
            return View();
        }

        [HttpPost]
        public IActionResult CreateArticle(ArticleWrap p)
        {
            using (PetDbContext db = new PetDbContext()) 
            {
                p.IsExist = true; //設定文章為存在狀態
                p.CreateAt = DateTime.Now; //設定文章的建立時間為目前時間

                db.Articles.Add(p.article);
                db.SaveChanges();
                return RedirectToAction("ArticleList");
            }

        }

        public IActionResult EditArticle(int? id)
        {
            using (PetDbContext db = new PetDbContext())
            {
                Article x = db.Articles.FirstOrDefault(p => p.ArticleId == id);
                if (x == null)
                {
                    return RedirectToAction("ArticleList");
                }

                ArticleWrap p = new ArticleWrap(); //創建一個ArticleWrap物件
                p.article = x;  //將從資料庫中查找到的文章賦值給ArticleWrap物件的article屬性

                return View(p);
            }
        }

        [HttpPost]
        public IActionResult EditArticle(ArticleWrap uiArticle)
        {
            using (PetDbContext db = new PetDbContext())
            {

                int targetId = uiArticle.article.ArticleId; //從傳入的ArticleWrap物件中獲取要編輯的文章ID

                Article dbArticle = db.Articles.FirstOrDefault(p => p.ArticleId == targetId); //從資料庫中查找要編輯的文章
            

                if (dbArticle != null && dbArticle.IsExist == true)
                {
                    dbArticle.Title = uiArticle.article.Title; //更新文章標題
                    dbArticle.Content = uiArticle.article.Content; //更新文章內容
                    dbArticle.LastEditTime = DateTime.Now; //更新最後編輯時間
                    db.SaveChanges(); //保存更改到資料庫

                }
                return RedirectToAction("ArticleList");
            }

        }

        [HttpPost] //刪除文章的動作通常使用POST方法來執行，以確保安全性和防止CSRF攻擊
        public IActionResult DeleteArticle(int? id)
        {
            if (id == null)
                return RedirectToAction("ArticleList");

            using (PetDbContext db = new PetDbContext())
            {
                Article x = db.Articles.FirstOrDefault(p => p.ArticleId == id);
                if (x != null)
                {
                    x.IsExist = false;
                    db.SaveChanges();
                }
                return RedirectToAction("ArticleList");
            }
        }


        public IActionResult ArticleImageList(KeywordViewModel vm)
        {
            PetDbContext db = new PetDbContext();

            IEnumerable<ArticleImage> datas = null; //宣告一個變數來存放查詢結果
            if (string.IsNullOrEmpty(vm.txtArticleId.ToString()))
            {
                datas = db.ArticleImages.Where(p => p.IsExist).ToList(); //查詢所有存在的文章圖片
            }
            else
            {
                datas = db.ArticleImages.Where(p => p.IsExist
                && (p.ArticleId == vm.txtArticleId
                )).ToList(); //根據搜尋條件查詢文章圖片
            }
            return View(datas);
        }





        public IActionResult EventList(KeywordViewModel vm) //活動管理頁面
        {
            PetDbContext db = new PetDbContext();
            vm.txtCategoryId = 1; //假設活動的CategoryId為1

            IEnumerable<Article> datas = null; //宣告一個變數來存放查詢結果

            datas = db.Articles.Where(p => p.IsExist
            && (p.CategoryId == vm.txtCategoryId) //篩選出活動類別的文章
            || (p.Title.Contains(vm.txtKeyword)
            || p.Content.Contains(vm.txtKeyword))
            || p.EventLocation.Contains(vm.txtKeyword)
                 ); //根據搜尋條件查詢文章

            return View(datas);

        }

    }
}
