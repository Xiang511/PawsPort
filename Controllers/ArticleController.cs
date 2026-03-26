using Microsoft.AspNetCore.Mvc;
using PawsPort.Models;
using PawsPort.ViewModels;
using PawsPort.Services;


namespace PawsPort.Controllers
{
    public class ArticleController : Controller
    {
        //public IActionResult Index()
        //{
        //    return View();
        //}



        public IActionResult ArticleList(KeywordViewModel Vm) //貼文管理頁面
        {

            using (PetDbContext Db = new PetDbContext()) //查詢完畢後就關閉
            {
                var Query = Db.Articles.Where(p => p.IsExist); //查詢所有存在的文章

                if (!string.IsNullOrEmpty(Vm.TxtKeyword))
                {
                    Query = Query.Where(p => p.Title.Contains(Vm.TxtKeyword)
                  || p.Content.Contains(Vm.TxtKeyword)); //根據搜尋條件查詢文章
                }

                var Datas = Query.ToList().Select(p => new ArticleWrap { article = p }); //將查詢結果轉換為ArticleWrap物件的列表

                return View(Datas);
            }

        }


        public IActionResult CreateArticle()
        {
            return View();
        }

        [HttpPost]
        public IActionResult CreateArticle(ArticleWrap p)
        {
            using (PetDbContext Db = new PetDbContext())
            {
                p.IsExist = true; //設定文章為存在狀態
                p.CreateAt = DateTime.Now; //設定文章的建立時間為目前時間
                Db.Articles.Add(p.article);
                Db.SaveChanges();

            
                return RedirectToAction("ArticleList");
            }

        }

        public IActionResult EditArticle(int? id)
        {
            using (PetDbContext Db = new PetDbContext())
            {
                Article x = Db.Articles.FirstOrDefault(p => p.ArticleId == id);
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
        public IActionResult EditArticle(ArticleWrap UiArticle)
        {
            using (PetDbContext Db = new PetDbContext())
            {

                //int targetId = uiArticle.article.ArticleId; //從傳入的ArticleWrap物件中獲取要編輯的文章ID

                Article DbArticle = Db.Articles.FirstOrDefault(p => p.ArticleId == UiArticle.ArticleId); //從資料庫中查找要編輯的文章


                if (DbArticle != null && DbArticle.IsExist == true)
                {

                    if (UiArticle.ArticleImage != null)
                    {
                        string ImageName = Guid.NewGuid().ToString() + Path.GetExtension(UiArticle.ArticleImage.FileName); //生成唯一的圖片名稱
                    }

                    DbArticle.Title = UiArticle.Title; //更新文章標題
                    DbArticle.Content = UiArticle.Content; //更新文章內容
                    DbArticle.CategoryId = UiArticle.CategoryId;
                    DbArticle.IsExist = UiArticle.IsExist;

                    DbArticle.LastEditTime = DateTime.Now; //更新最後編輯時間
                    Db.SaveChanges(); //保存更改到資料庫

                }
                return RedirectToAction("ArticleList");
            }

        }

        [HttpPost] //刪除文章的動作通常使用POST方法來執行，以確保安全性和防止CSRF攻擊
        public IActionResult DeleteArticle(int? id)
        {
            if (id == null)
                return RedirectToAction("ArticleList");

            using (PetDbContext Db = new PetDbContext())
            {
                Article x = Db.Articles.FirstOrDefault(p => p.ArticleId == id);
                if (x != null)
                {
                    x.IsExist = false;
                    Db.SaveChanges();
                }
                return RedirectToAction("ArticleList");
            }
        }


        public IActionResult ArticleImageList(KeywordViewModel Vm)
        {
            PetDbContext Db = new PetDbContext();

            IEnumerable<ArticleImage> Datas = null; //宣告一個變數來存放查詢結果
            if (string.IsNullOrEmpty(Vm.TxtArticleId.ToString()))
            {
                Datas = Db.ArticleImages.Where(p => p.IsExist).ToList(); //查詢所有存在的文章圖片
            }
            else
            {
                Datas = Db.ArticleImages.Where(p => p.IsExist
                && (p.ArticleId == Vm.TxtArticleId
                )).ToList(); //根據搜尋條件查詢文章圖片
            }
            return View(Datas);
        }





        public IActionResult EventList(KeywordViewModel vm) //活動管理頁面
        {
            PetDbContext db = new PetDbContext();
            vm.TxtCategoryId = 1; //假設活動的CategoryId為1

            IEnumerable<Article> datas = null; //宣告一個變數來存放查詢結果

            datas = db.Articles.Where(p => p.IsExist
            && (p.CategoryId == vm.TxtCategoryId) //篩選出活動類別的文章
            || (p.Title.Contains(vm.TxtKeyword)
            || p.Content.Contains(vm.TxtKeyword))
            || p.EventLocation.Contains(vm.TxtKeyword)
                 ); //根據搜尋條件查詢文章

            return View(datas);

        }

    }
}
