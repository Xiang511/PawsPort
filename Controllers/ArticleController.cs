using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using PawsPort.Models;
using PawsPort.Services;
using PawsPort.ViewModels;


namespace PawsPort.Controllers
{
    public class ArticleController : Controller
    {

        IWebHostEnvironment _Env = null;

        public ArticleController(IWebHostEnvironment p)
        {
            _Env = p;
        }



        public IActionResult ArticleList(ArticleListViewModel vm) //貼文管理頁面
        {

            using (PetDbContext db = new PetDbContext())
            {
                // 1. 基本查詢：只抓存在的文章，並包含 User 與 Category 資料
                var query = db.Articles.Where(p => p.IsExist);

                // 2. 關鍵字篩選 (標題、內容、作者)
                if (!string.IsNullOrEmpty(vm.TxtKeyword))
                {
                    var MatchUserIDs = db.UserTables.Where(u => u.Name.Contains(vm.TxtKeyword))
                        .Select(u => u.UserId).ToList();

                    query = query.Where(p => p.Title.Contains(vm.TxtKeyword)
                                          || p.Content.Contains(vm.TxtKeyword)
                                          || MatchUserIDs.Contains(p.UserId));
                }

                // 3. 計算總筆數
                vm.TotalCount = query.Count();

                // 4. 在 Select 時「現場去別張表抓資料」
                vm.ArticleItem = query
                    .OrderByDescending(p => p.CreateAt)
                    .Skip((vm.CurrentPage - 1) * vm.PageSize)
                    .Take(vm.PageSize)
                    .Select(p => new ArticleItemViewModel
                    {
                        ArticleId = p.ArticleId,
                        UserId = p.UserId,
                        Title = p.Title,
                        CreateAt = p.CreateAt,
                        ViewCount = p.ViewCount,
                        IsExist = p.IsExist,

                        // 【抓作者名稱】去 UserTable 找 ID 一樣的那個人，取其 Name
                        AuthorName = db.UserTables
                            .Where(u => u.UserId == p.UserId)
                            .Select(u => u.Name)
                            .FirstOrDefault() ?? "未知作者",

                        // 【抓分類名稱】去 Categories 找 ID 一樣的那組，取其 Name
                        CategoryName = db.Categories
                            .Where(c => c.CategoryId == p.CategoryId)
                            .Select(c => c.CategoryName)
                            .FirstOrDefault() ?? "未分類",

                        // 【算留言數】去 Comments 找這篇文章的留言數量
                        CommentCount = db.Comments.Count(c => c.ArticleId == p.ArticleId && c.IsExist),

                        // 【算書籤數】去 Bookmarks 找這篇文章的收藏數量
                        BookmarkCount = db.Bookmarks.Count(b => b.ArticleId == p.ArticleId)
                    })
                    .ToList();

                return View(vm);
            }
        }

       


        public IActionResult CreateArticle()
        {
            return View();
        }

        [HttpPost]
        public IActionResult CreateArticle(ArticleWrap p)
        {
            FileService F = new FileService(_Env);
            if (p.ImageFiles != null && p.ImageFiles.Count > 0)
            {
                List<string> ErrorFile = new List<string>();

                foreach (IFormFile File in p.ImageFiles)
                {
                    if (!F.IsValidImage(File))

                        ErrorFile.Add(File.FileName);
                }
                if (ErrorFile.Count > 0)
                {
                    string AllErrorFiles = string.Join(", ", ErrorFile);
                    ModelState.AddModelError("ImageFiles", $"檔案 {AllErrorFiles} 格式錯誤");
                    TempData["ErrorMsg"] = $"圖片格式不符：{AllErrorFiles}，上傳已取消。";

                    return View(p);
                }
            }
            using (PetDbContext db = new PetDbContext())
            {
                p.article.IsExist = true; //設定文章為存在狀態
                p.article.CreateAt = DateTime.Now; //設定文章的建立時間為目前時間
                db.Articles.Add(p.article);
                db.SaveChanges(); //建立文章並存到資料庫

                if (p.ImageFiles != null && p.ImageFiles.Count > 0)
                {
                    foreach (IFormFile File in p.ImageFiles)
                    {
                        string ImageName = F.SaveImage(File); //將圖片保存到伺服器並獲取圖片名稱
                        if (ImageName != null)
                        {
                            ArticleImage Img = new ArticleImage();
                            Img.ArticleId = p.article.ArticleId; //將圖片與文章關聯
                            Img.Image = "/Image/" + ImageName; //存入路徑
                            db.ArticleImages.Add(Img); //圖片資訊存入資料庫
                        }
                    }
                    db.SaveChanges(); //保存更改到資料庫
                }
            }
            return RedirectToAction("ArticleList");
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
        public IActionResult EditArticle(ArticleWrap UiArticle)
        {
            using (PetDbContext db = new PetDbContext())
            {

                Article dbArticle = db.Articles.FirstOrDefault(p => p.ArticleId == UiArticle.ArticleId);
                //從資料庫中查找要編輯的文章


                if (dbArticle != null && dbArticle.IsExist == true)
                {
                    dbArticle.Title = UiArticle.Title; //更新文章標題
                    dbArticle.Content = UiArticle.Content; //更新文章內容
                    dbArticle.CategoryId = UiArticle.CategoryId;
                    dbArticle.IsExist = UiArticle.IsExist;

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
                    var ImageList =db.ArticleImages.Where(p => p.ArticleId == id).ToList();
                    foreach (var img in ImageList)
                    {
                        img.IsExist = false; //將與該文章相關的圖片標記為不存在
                    }
                    db.SaveChanges();
                }
            }
            return RedirectToAction("ArticleList");
        }


        public IActionResult ArticleImageList(KeywordViewModel vm)
        {
            PetDbContext db = new PetDbContext();

            IEnumerable<ArticleImage> Datas = null; //宣告一個變數來存放查詢結果
            if (string.IsNullOrEmpty(vm.TxtArticleId.ToString()))
            {
                Datas = db.ArticleImages.Where(p => p.IsExist).ToList(); //查詢所有存在的文章圖片
            }
            else
            {
                Datas = db.ArticleImages.Where(p => p.IsExist
                && (p.ArticleId == vm.TxtArticleId
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
