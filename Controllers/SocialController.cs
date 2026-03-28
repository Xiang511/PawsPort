using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using PawsPort.Models;
using PawsPort.ViewModels;

namespace PawsPort.Controllers
{
    public class SocialController : Controller
    {
        //public IActionResult Index()
        //{
        //    return View();
        //}

        public IActionResult SocialList(SocialListViewModel vm)
        {
            using (PetDbContext db = new PetDbContext())
            {
                //首先要按照使用者來排序
                var Query = db.UserTables.Where(p => p.Status == true);
                //查詢所有存在的使用者(假設status true=存在)

                if (!string.IsNullOrEmpty(vm.TxtKeyword)) //如果關鍵字不為空
                {
                    Query = Query.Where(p => p.Name.Contains(vm.TxtKeyword)); //搜尋使用者名稱
                }

                //顯示他有幾個追蹤者，和他追蹤了幾個人
                //顯示有多少發文，發文有幾個留言和幾個書籤
                var ItemQuery = Query.Select(p => new SocialItemViewModel
                //將查到的使用者轉換成SocialItemViewModel p
                {
                    UserId = p.UserId,
                    UserName = p.Name,
                    FollowerCount = db.Followings.Count(f => f.FollowingId == p.UserId), //追蹤者數量
                    FollowingCount = db.Followings.Count(f => f.UserId == p.UserId), //追蹤的使用者數量
                    PostCount = db.Articles.Count(a => a.UserId == p.UserId), //發文數量

                    //留言雙向數據
                    MyCommentCount = db.Comments.Count(c => c.UserId == p.UserId), //留言數量
                    ReceivedCommentCount = db.Comments.Count
                    (c => db.Articles.Any(a => a.ArticleId == c.ArticleId && a.UserId == p.UserId)),
                    //收到的留言數量(dbArticle裡符合(貼文表ID==留言表ID && 貼文表UserID == 查到的UserID))
                    //未來須加上FK

                    //書籤雙向數據
                    MyBookmarkCount = db.Bookmarks.Count(b => b.UserId == p.UserId), //書籤數量
                    ReceivedBookmarkCount = db.Bookmarks.Count
                    (b => db.Articles.Any(a => a.ArticleId == b.ArticleId && a.UserId == p.UserId)),
                    //收到的書籤數量(dbArticle裡符合(貼文表ID==書籤表ID && 貼文表UserID == 查到的UserID))
                    //未來須加上FK

                    //人氣分數算法
                    PopularityScore = (db.Followings.Count(f => f.FollowingId == p.UserId) * 10) +
                 (db.Bookmarks.Count
                 (b => db.Articles.Any(a => a.ArticleId == b.ArticleId && a.UserId == p.UserId)) * 5) +
                 (db.Comments.Count
                 (c => db.Articles.Any(a => a.ArticleId == c.ArticleId && a.UserId == p.UserId)) * 2)
                });

                switch (vm.SortBy) //根據vm.SortBy的值來決定排序方式
                {
                    case "PostCount": //發文數量排序
                        if (vm.SortOrder == "desc")
                        {
                            ItemQuery = ItemQuery.OrderByDescending(i => i.PostCount);
                            //按照發文數量由多到少排序
                        }
                        else
                        {
                            ItemQuery = ItemQuery.OrderBy(i => i.PostCount);
                            //按照發文數量由少到多排序
                        }
                        break;
                    case "FollowerCount": //追蹤者數量排序
                        if (vm.SortOrder == "desc")
                        {
                            ItemQuery = ItemQuery.OrderByDescending(i => i.FollowerCount);
                            //按照追蹤者數量由多到少排序
                        }
                        else
                        {
                            ItemQuery = ItemQuery.OrderBy(i => i.FollowerCount);
                            //按照追蹤者數量由少到多排序
                        }
                        break;
                    case "Popularity":
                        if (vm.SortOrder == "desc")
                        {
                            ItemQuery = ItemQuery.OrderByDescending(i => i.PopularityScore);
                            //按照人氣分數由多到少排序
                        }
                        else
                        {
                            ItemQuery = ItemQuery.OrderBy (i => i.PopularityScore);
                            //按照人氣分數由少到多排序
                        }
                        break;
                    case "Default": //預設排序，按照使用者ID由小到大排序
                        ItemQuery = ItemQuery.OrderBy(i => i.UserId);
                        break;
                }

                //切出當前要看的那一頁
                vm.TotalCount = ItemQuery.Count(); //總資料筆數
                vm.SocialItems = ItemQuery.Skip((vm.CurrentPage - 1) * vm.PageSize)
                    .Take(vm.PageSize).ToList();
                //ItemQuery(查詢結果).跳過((當下頁數-1)*每頁筆數).取(每頁筆數).轉換成列表






                //然後要有一個搜尋功能，可以搜尋使用者名稱，或者是文章內容
                //要有一個分頁功能，每頁顯示10筆資料
                //要有一個排序功能，可以按照使用者名稱，或者是文章數量，或者是追蹤者數量來排序
                //要有一個篩選功能，可以篩選出有多少追蹤者的使用者，或者是有多少文章的使用者
                //要有一個詳細資料頁面，點擊使用者名稱可以進入詳細資料頁面，顯示使用者的基本資料，追蹤者和追蹤的使用者列表，發文列表，每篇文章的留言和書籤數量
                //

                return View(vm);
            }

        }


        public IActionResult Details(int id)
        {
            using (PetDbContext db = new PetDbContext())
            {
                var user = db.UserTables.FirstOrDefault(u => u.UserId == id); //先根據傳入id找使用者
                if (user == null)
                    return NotFound(); //如果找不到使用者，回傳404

                var vm = new SocialDetailsViewModel //將使用者資料轉換成SocialDetailsViewModel
                {
                    UserId = user.UserId,
                    UserName = user.Name,

                    FollowerCount = db.Followings.Count(f => f.FollowingId == user.UserId), //追蹤者數量
                    FollowingCount = db.Followings.Count(f => f.UserId == user.UserId), //追蹤的使用者數量

                    PostCount = db.Articles.Count(a => a.UserId == user.UserId), //發文數量

                    MyCommentCount = db.Comments.Count(c => c.UserId == user.UserId), //留言數量
                    ReceivedCommentCount = db.Comments.Count
                    (c => db.Articles.Any(a => a.ArticleId == c.ArticleId && a.UserId == user.UserId)),
                    //收到的留言數量

                    MyBookmarkCount = db.Bookmarks.Count(b => b.UserId == user.UserId), //書籤數量
                    ReceivedBookmarkCount = db.Bookmarks.Count
                    (b => db.Articles.Any(a => a.ArticleId == b.ArticleId && a.UserId == user.UserId)),
                    //收到的書籤數量

                    RecentArticles = db.Articles.Where(a => a.UserId == id)
                    .OrderByDescending(a => a.CreateAt).Take(5)
                    .Select(a => new ArticleSummary
                    {
                        ArticleId = a.ArticleId,
                        Title = a.Title,
                        CreateAt = a.CreateAt,
                    }).ToList()
                };
                return View(vm);
            }

        }

    }
}
