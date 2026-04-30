using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using PawsPort.Dtos;
using PawsPort.Models;
using PawsPort.Services;
using PawsPort.ViewModels;
using Serilog;
using Serilog.Events;


namespace PawsPort.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Produces("application/json")]
    public class ArticleController : ApiControllerBase
    {
        //注入資料庫和service
        private readonly PetDbContext _context;
        private readonly ArticleService _articleService;

        private readonly IWebHostEnvironment _Env = null;

        public ArticleController(IWebHostEnvironment p, PetDbContext context, ArticleService articleService)
        {
            _Env = p;
            _context = context;
            _articleService = articleService;
        }



        //public IActionResult ArticleList(ArticleListViewModel vm) //貼文管理頁面
        //{

        //    using (PetDbContext db = new PetDbContext())
        //    {
        //        // 1. 基本查詢：只抓存在的文章，並包含 User 與 Category 資料
        //        var query = db.Articles.Where(p => p.IsExist);

        //        // 2. 關鍵字篩選 (標題、內容、作者)
        //        if (!string.IsNullOrEmpty(vm.TxtKeyword))
        //        {
        //            var MatchUserIDs = db.UserTables.Where(u => u.Name.Contains(vm.TxtKeyword))
        //                .Select(u => u.UserId).ToList();

        //            query = query.Where(p => p.Title.Contains(vm.TxtKeyword)
        //                                  || p.Content.Contains(vm.TxtKeyword)
        //                                  || MatchUserIDs.Contains(p.UserId));
        //        }

        //        // 3. 計算總筆數
        //        vm.TotalCount = query.Count();

        //        // 4. 在 Select 時「現場去別張表抓資料」
        //        vm.ArticleItem = query
        //            .OrderByDescending(p => p.CreateAt)
        //            .Skip((vm.CurrentPage - 1) * vm.PageSize)
        //            .Take(vm.PageSize)
        //            .Select(p => new ArticleItemViewModel
        //            {
        //                ArticleId = p.ArticleId,
        //                UserId = p.UserId,
        //                Title = p.Title,
        //                CreateAt = p.CreateAt,
        //                ViewCount = p.ViewCount,
        //                IsExist = p.IsExist,

        //                // 【抓作者名稱】去 UserTable 找 ID 一樣的那個人，取其 Name
        //                AuthorName = db.UserTables
        //                    .Where(u => u.UserId == p.UserId)
        //                    .Select(u => u.Name)
        //                    .FirstOrDefault() ?? "未知作者",

        //                // 【抓分類名稱】去 Categories 找 ID 一樣的那組，取其 Name
        //                CategoryName = db.Categories
        //                    .Where(c => c.CategoryId == p.CategoryId)
        //                    .Select(c => c.CategoryName)
        //                    .FirstOrDefault() ?? "未分類",

        //                // 【算留言數】去 Comments 找這篇文章的留言數量
        //                CommentCount = db.Comments.Count(c => c.ArticleId == p.ArticleId && c.IsExist),

        //                // 【算書籤數】去 Bookmarks 找這篇文章的收藏數量
        //                BookmarkCount = db.Bookmarks.Count(b => b.ArticleId == p.ArticleId)
        //            })
        //            .ToList();

        //        return View(vm);
        //    }
        //}



        [HttpPost]
        public async Task<IActionResult> Article([FromBody] ArticleSaveDTO articleDto)
        {
            if (!ModelState.IsValid)
            {
                return Failure("VALIDATION_ERROR", "資料驗證失敗", 400);
            }
            try
            {
                var result = await _articleService.CreateArticleAsync(articleDto);
                return Success(result, "文章建立成功", 200);
                //**跳轉到文章詳細頁面
            }
            catch (Exception ex)
            {
                return Failure("INTERNAL_ERROR", "伺服器內部錯誤", 500);
            }
        }




        [HttpPut("{id}")]
        public async Task<IActionResult> Article(int id, [FromBody] ArticleSaveDTO articleDto)
        {
         
            if(id<=0)
            {
                return Failure("INVALID_ID", "無效的文章編號", 400);
            }
            //如果一致，呼叫service更新文章
            var result = await _articleService.UpdateArticleAsync(id, articleDto);
            if(result == null)
            {
                //回傳找不到該編號文章
                return Failure("ARTICLE_NOT_FOUND", "找不到該文章", 404);
            }
            else
            {
                return Success(result, "文章更新成功", 200);
                //**跳轉到文章詳細頁面
            }

        }

        [HttpDelete("{id}")] 
        public async Task<IActionResult> Delete(int id)
        {
            //檢查id是否有效
            if (id <= 0) return Failure("INVALID_ID", "無效的文章編號", 400);

            //有效的話呼叫service
            var result = await _articleService.DeleteArticleAsync(id);
            //若service回傳false，代表找不到該文章
            //若service回傳true，代表刪除成功
            if (result == false)
            {
                //回傳找不到該編號文章
                return Failure("ARTICLE_NOT_FOUND", "找不到該文章", 404);
            }
            else
            {
                return Success(result, "文章刪除成功", 200);
            }
        }


        //public IActionResult ArticleImageList(KeywordViewModel vm)
        //{
        //    PetDbContext db = new PetDbContext();

        //    IEnumerable<ArticleImage> Datas = null; //宣告一個變數來存放查詢結果
        //    if (string.IsNullOrEmpty(vm.TxtArticleId.ToString()))
        //    {
        //        Datas = db.ArticleImages.Where(p => p.IsExist).ToList(); //查詢所有存在的文章圖片
        //    }
        //    else
        //    {
        //        Datas = db.ArticleImages.Where(p => p.IsExist
        //        && (p.ArticleId == vm.TxtArticleId
        //        )).ToList(); //根據搜尋條件查詢文章圖片
        //    }
        //    return View(Datas);
        //}


        //public IActionResult EventList(KeywordViewModel vm) //活動管理頁面
        //{
        //    PetDbContext db = new PetDbContext();
        //    vm.TxtCategoryId = 1; //假設活動的CategoryId為1

        //    IEnumerable<Article> datas = null; //宣告一個變數來存放查詢結果

        //    datas = db.Articles.Where(p => p.IsExist
        //    && (p.CategoryId == vm.TxtCategoryId) //篩選出活動類別的文章
        //    || (p.Title.Contains(vm.TxtKeyword)
        //    || p.Content.Contains(vm.TxtKeyword))
        //    || p.EventLocation.Contains(vm.TxtKeyword)
        //         ); //根據搜尋條件查詢文章

        //    return View(datas);

        //}

    }
}
