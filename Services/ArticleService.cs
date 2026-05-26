
using Microsoft.EntityFrameworkCore;
using Org.BouncyCastle.Crypto;
using PawsPort.Dtos;
using PawsPort.Models;
using System.Diagnostics;
using System.Text.RegularExpressions;



namespace PawsPort.Services
{
    public class ArticleService
    {
        private readonly PetDbContext _context;

        public ArticleService(PetDbContext context)
        {
            _context = context;
        }



        //=====新增文章=====
        public async Task<int> CreateArticleAsync(ArticleSaveDTO articleDto)
        {
            //將ArticleDTO轉換為Article實體
            var ArticleEntity = new Article
            {
                UserId = articleDto.UserId,
                CategoryId = articleDto.CategoryId,
                Title = articleDto.Title,
                Content = articleDto.Content,
                Status = articleDto.Status,

                EventStartDate = articleDto.EventStartDate,
                EventEndDate = articleDto.EventEndDate,
                EventLocation = articleDto.EventLocation,

                CreateAt = DateTime.UtcNow,
                LastEditTime = DateTime.UtcNow,
                ViewCount = 0,
                IsExist = true,
                IsActive = true,

                ReportedCount = 0,
                LastReported = null,
                DeleteTypeId = null,
                DeleteNote = null

            };

            //將Article實體添加到資料庫
            _context.Articles.Add(ArticleEntity);
            //保存更改到資料庫，拿到ArticleId
            await _context.SaveChangesAsync();

            var imageUrls = ExtractImageUrlsFromContent(articleDto.Content);

            if (imageUrls.Any())
            {
                var articleImages = imageUrls.Select((url, index) => new ArticleImage
                {
                    ArticleId = ArticleEntity.ArticleId,
                    ImageUrl = url,
                    SortOrder = index + 1,

                    CreateAt = DateTime.UtcNow,
                    LastEditTime = DateTime.UtcNow,
                    IsExist = true,
                    IsActive = true
                }).ToList();

                _context.ArticleImages.AddRange(articleImages);
                await _context.SaveChangesAsync();
            }

            //處理標籤
            var Tags = await PrepareTagsAsync(articleDto.TagNames);
            //建立文章-標籤關聯
            var Maps = Tags.Select(t => new ArticleTagMap
            {
                ArticleId = ArticleEntity.ArticleId,
                TagId = t.TagId,
            }).ToList();

            //將ArticleTagMap存入資料庫
            _context.ArticleTagMaps.AddRange(Maps);

            //存標籤關聯
            await _context.SaveChangesAsync();

            //將新增的Article實體轉換回ArticleDTO
            //articleDto.ArticleId = ArticleEntity.ArticleId;

            return ArticleEntity.ArticleId;
        }


        //=====修改文章=====
        public async Task<int?> UpdateArticleAsync(int id, ArticleSaveDTO articleDto, int userId)
        {
            //比對articleDto.articleId和資料庫裡的ArticleId，去資料庫撈出對應的文章實體
            var ArticleEntity = await _context.Articles.FirstOrDefaultAsync(a =>
            a.ArticleId == id &&
            a.UserId == userId &&
            a.IsExist == true &&
            a.IsActive == true);

            if (ArticleEntity == null)
            {
                //查無此文章，拋出例外或回傳null
                return null;
            }
            else
            {
                //找到文章實體
                //更新文章內容
                ArticleEntity.LastEditTime = DateTime.UtcNow;
                ArticleEntity.CategoryId = articleDto.CategoryId;
                ArticleEntity.Title = articleDto.Title;
                ArticleEntity.Content = articleDto.Content;
                ArticleEntity.Status = articleDto.Status;   //狀態(0:草稿,1:公開,2:封鎖)

                ArticleEntity.EventStartDate = articleDto.EventStartDate;
                ArticleEntity.EventEndDate = articleDto.EventEndDate;
                ArticleEntity.EventLocation = articleDto.EventLocation;

                //先撈出原本的標籤關聯
                var ExistingMaps = await _context.ArticleTagMaps.Where(m => m.ArticleId == id).ToListAsync();
                //這裡應該要改成停用舊的關聯(isExist = false)
                foreach (var map in ExistingMaps)
                {
                    map.IsExist = false;
                }
                //可能要加上一個比對新舊標籤的功能
                //打包新的標籤list
                var NewTags = await PrepareTagsAsync(articleDto.TagNames);

                //建立新的標籤關聯(一個map = tagID + articleID)
                var NewMaps = NewTags.Select(t => new ArticleTagMap
                {
                    ArticleId = id,
                    TagId = t.TagId

                }).ToList();
                //加入新的標籤關聯
                _context.ArticleTagMaps.AddRange(NewMaps);
                //存回資料庫
                await _context.SaveChangesAsync();
            }
            return ArticleEntity.ArticleId;
        }


        //=====刪除文章=====
        public async Task<bool> DeleteArticleAsync(int id)
        {
            //去資料庫撈對應id的文章實體和標籤關聯
            var ArticleEntity = await _context.Articles.FirstOrDefaultAsync(a => a.ArticleId == id);

            //如果找不到，回傳false
            //如果找到，將它的IsExist改為false，存回資料庫，回傳true
            if (ArticleEntity == null || ArticleEntity.IsExist == false) return false;

            //有找到的話，先找出標籤關聯
            var Maps = await _context.ArticleTagMaps.Where(m => m.ArticleId == id && m.IsExist == true).ToListAsync();

            //把文章本體和標籤關聯的IsExist改為false
            ArticleEntity.IsExist = false;
            ArticleEntity.LastEditTime = DateTime.UtcNow;

            foreach (var map in Maps)
            {
                map.IsExist = false;
                map.LastEditTime = DateTime.UtcNow;
            }

            await _context.SaveChangesAsync();
            return true;
        }



        //=====文章列表(所有文章)=====
        //使用參數來篩選，預設為null(不篩選)，有值的話才篩選
        //status: 0:草稿,1:公開,2:私人
        public async Task<List<ArticleListDTO>> GetAllArticlesAsync(int? status = null, bool? isActive = null, int? userId = null)
        {
            //撈文章+使用者名稱+分類名稱，轉換成ArticleListDTO
            var query = from a in _context.Articles
                        join u in _context.UserTables on a.UserId equals u.UserId
                        join c in _context.Categories on a.CategoryId equals c.CategoryId
                        where a.IsExist == true
                        select new { a, u, c };
            //加上篩選條件
            if (status.HasValue)
            {
                query = query.Where(x => x.a.Status == status.Value);
            }

            if (isActive.HasValue)
            {
                query = query.Where(x => x.a.IsActive == isActive.Value);
            }

            if (userId.HasValue)
            {
                query = query.Where(x => x.a.UserId == userId.Value);
            }
            //
            var ArticleList = await query.Select(x => new
            {
                DTO = new ArticleListDTO
                {
                    ArticleId = x.a.ArticleId,
                    CategoryId = x.c.CategoryId,
                    Title = x.a.Title,
                    Summary = null,
                    CreateAt = x.a.CreateAt,
                    LastEditTime = x.a.LastEditTime,
                    Status = x.a.Status,
                    ViewCount = x.a.ViewCount,
                    EventStartDate = x.a.EventStartDate,
                    EventEndDate = x.a.EventEndDate,
                    EventLocation = x.a.EventLocation,
                    CategoryName = x.c.CategoryName,
                    UserName = x.u.Name,

                    MainImageUrl = _context.ArticleImages
                    .Where(img =>
                    img.ArticleId == x.a.ArticleId &&
                    img.IsExist == true &&
                    img.IsActive == true)
                    .OrderBy(img => img.SortOrder)
                    .ThenBy(img => img.ImageId)
                    .Select(img => img.ImageUrl)
                    .FirstOrDefault(),
                },
                OriginalContent = x.a.Content
            }).ToListAsync();

            var finalArticleList = ArticleList.Select(x => x.DTO).ToList();
            var ArticleIds = finalArticleList.Select(a => a.ArticleId).ToList();

            //撈出對應的tagid
            var Tags = await (from m in _context.ArticleTagMaps
                              join t in _context.Tags on m.TagId equals t.TagId
                              where ArticleIds.Contains(m.ArticleId) && m.IsExist == true
                              select new
                              {
                                  m.ArticleId,
                                  t.TagName
                              }).ToListAsync();

            for (int i = 0; i < ArticleList.Count; i++)
            {
                var item = ArticleList[i];

                // 塞入標籤
                item.DTO.TagNames = Tags.Where(t => t.ArticleId == item.DTO.ArticleId).Select(t => t.TagName).ToList();

                // 處理 Summary：拿剛才順便查出來的內文，丟進小工具去標籤並截斷 60 字
                item.DTO.Summary = GetTextSummary(item.OriginalContent, 60);
            }

            return finalArticleList;
        }


        //=====取得文章詳細(文章id)=====
        public async Task<ArticleDetailDTO?> GetArticleDetailAsync(int id)
        {
            Console.WriteLine($"開始取得文章詳細：{id}");
            // 先把實體文章撈出來，將資料庫的 ViewCount 真正 +1 並儲存
            var articleEntity = await _context.Articles
                .FirstOrDefaultAsync(a => a.ArticleId == id && a.IsExist == true && a.Status == 1);

            if (articleEntity == null)
            {
                return null; // 文章不存在就直接結束，省去後面不必要的 Join 查詢
            }

            articleEntity.ViewCount += 1;
            await _context.SaveChangesAsync();

            // 1. 先查文章主體 + Category + UserTable
            var articleDetail = await (
                from a in _context.Articles
                join c in _context.Categories
                    on a.CategoryId equals c.CategoryId
                join u in _context.UserTables
                    on a.UserId equals u.UserId
                where a.ArticleId == id && a.IsExist == true
                select new ArticleDetailDTO
                {
                    ArticleId = a.ArticleId,
                    Title = a.Title,
                    Content = a.Content,
                    CreateAt = a.CreateAt,
                    LastEditTime = a.LastEditTime,
                    ViewCount = a.ViewCount,
                    EventStartDate = a.EventStartDate,
                    EventEndDate = a.EventEndDate,
                    EventLocation = a.EventLocation,
                    UserId = a.UserId,
                    CategoryId = a.CategoryId,

                    // Category
                    CategoryName = c.CategoryName,

                    // UserTable
                    UserName = u.Name,
                    UserPhoto = u.Photo,

                    // 下面先給預設，等等再補
                    BookmarkCount = 0,
                    CoverImageUrl = null,
                    ImageUrls = new List<string>(),
                    Tags = new List<string>()
                }
            ).FirstOrDefaultAsync();

            Console.WriteLine("主文章查詢完成");
            // 理論上前面查得到，這裡一定不為 null，但留著做安全檢查
            if (articleDetail == null)
            {
                return null;
            }

            // 2. 查收藏數 Bookmark
            articleDetail.BookmarkCount = await _context.Bookmarks
                .CountAsync(b => b.ArticleId == id && b.IsExist == true); // 加上 IsExist 判斷，避免算到已被取消收藏的
            Console.WriteLine("收藏數查詢完成");
            // 3. 查封面圖 ArticleImage
            articleDetail.CoverImageUrl = await _context.ArticleImages
                .Where(img => img.ArticleId == id && img.SortOrder == 1 && img.IsExist == true)
                .Select(img => img.ImageUrl)
                .FirstOrDefaultAsync();
            Console.WriteLine("封面圖查詢完成");
            // 4. 查全部圖片 ArticleImage
            articleDetail.ImageUrls = await _context.ArticleImages
                .Where(img => img.ArticleId == id && img.IsExist == true)
                .OrderBy(img => img.SortOrder)
                .Select(img => img.ImageUrl)
                .ToListAsync();
            Console.WriteLine("圖片清單查詢完成");
            // 5. 查標籤 ArticleTagMap
            articleDetail.Tags = await (
                from map in _context.ArticleTagMaps
                join tag in _context.Tags
                    on map.TagId equals tag.TagId
                where map.ArticleId == id && map.IsExist == true
                select tag.TagName
            ).ToListAsync();
            Console.WriteLine("標籤查詢完成");
            return articleDetail;
        }

        //=====取得目前登入會員的草稿列表=====
        public async Task<List<ArticleListDTO>> GetDraftArticlesAsync(int userId)
        {
            var draftList = await (
                from a in _context.Articles
                join u in _context.UserTables on a.UserId equals u.UserId
                join c in _context.Categories on a.CategoryId equals c.CategoryId
                where a.UserId == userId
                      && a.Status == 0
                      && a.IsExist == true
                      && a.IsActive == true
                orderby a.LastEditTime descending
                select new
                {
                    DTO = new ArticleListDTO
                    {
                        ArticleId = a.ArticleId,
                        CategoryId = a.CategoryId,
                        Title = a.Title,
                        Summary = null,
                        CreateAt = a.CreateAt,
                        LastEditTime = a.LastEditTime,
                        Status = a.Status,
                        ViewCount = a.ViewCount,
                        EventStartDate = a.EventStartDate,
                        EventEndDate = a.EventEndDate,
                        EventLocation = a.EventLocation,
                        CategoryName = c.CategoryName,
                        UserName = u.Name,

                        MainImageUrl = _context.ArticleImages
                            .Where(img =>
                                img.ArticleId == a.ArticleId &&
                                img.IsExist == true &&
                                img.IsActive == true)
                            .OrderBy(img => img.SortOrder)
                            .Select(img => img.ImageUrl)
                            .FirstOrDefault()
                    },
                    OriginalContent = a.Content
                }
            ).ToListAsync();

            foreach (var item in draftList)
            {
                item.DTO.Summary = GetTextSummary(item.OriginalContent, 60);
            }

            return draftList.Select(x => x.DTO).ToList();
        }

        //===取得草稿詳細(id)===
        public async Task<ArticleDetailDTO?> GetDraftArticleDetailAsync(int id, int userId)
        {
            var articleDetail = await (
                from a in _context.Articles
                join c in _context.Categories on a.CategoryId equals c.CategoryId
                join u in _context.UserTables on a.UserId equals u.UserId
                where a.ArticleId == id
                      && a.UserId == userId
                      && a.Status == 0
                      && a.IsExist == true
                      && a.IsActive == true
                select new ArticleDetailDTO
                {
                    ArticleId = a.ArticleId,
                    Title = a.Title,
                    Content = a.Content,
                    CreateAt = a.CreateAt,
                    LastEditTime = a.LastEditTime,
                    ViewCount = a.ViewCount,

                    EventStartDate = a.EventStartDate,
                    EventEndDate = a.EventEndDate,
                    EventLocation = a.EventLocation,

                    UserId = a.UserId,
                    CategoryId = a.CategoryId,

                    CategoryName = c.CategoryName,
                    UserName = u.Name,
                    UserPhoto = u.Photo,

                    BookmarkCount = 0,
                    CoverImageUrl = null,
                    ImageUrls = new List<string>(),
                    Tags = new List<string>()
                }
            ).FirstOrDefaultAsync();

            if (articleDetail == null)
            {
                return null;
            }

            articleDetail.Tags = await (
                from map in _context.ArticleTagMaps
                join tag in _context.Tags on map.TagId equals tag.TagId
                where map.ArticleId == id && map.IsExist == true
                select tag.TagName
            ).ToListAsync();

            articleDetail.CoverImageUrl = await _context.ArticleImages
                .Where(img =>
                    img.ArticleId == id &&
                    img.IsExist == true &&
                    img.IsActive == true)
                .OrderBy(img => img.SortOrder)
                .Select(img => img.ImageUrl)
                .FirstOrDefaultAsync();

            articleDetail.ImageUrls = await _context.ArticleImages
                .Where(img =>
                    img.ArticleId == id &&
                    img.IsExist == true &&
                    img.IsActive == true)
                .OrderBy(img => img.SortOrder)
                .Select(img => img.ImageUrl)
                .ToListAsync();

            return articleDetail;
        }

        //=====查詢文章(關鍵字)=====

        //=====查詢文章(分類)=====

        //=====查詢文章(標籤)=====

        //=====查詢文章(使用者)=====

        //=====活動列表=====

        //=====處理標籤 //private=====
        private async Task<List<Tag>> PrepareTagsAsync(List<string> TagNames)
        {
            //先檢查有無TagNames
            if (TagNames == null || !TagNames.Any())
                return new List<Tag>();

            //去除空白和重複的標籤(不分大小寫)
            var UniqueTagNames = TagNames
                .Select(t => t.Replace("#", "").Trim())
                .Where(t => !string.IsNullOrEmpty(t))
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToList();

            //去資料庫撈出所有tag表中名稱有包含在輸入的tagnames裡的資料，轉換成list<tag>
            var ExistingTags = await _context.Tags
                .Where(t => UniqueTagNames.Contains(t.TagName)).ToListAsync();

            //把撈到的tag資料轉換成tagname
            var ExistingNames = ExistingTags.Select(t => t.TagName);

            //減去已經存在的tagname(不分大小寫)，剩下的轉換成新的tag
            var NewTagsList = UniqueTagNames
                .Except(ExistingNames, StringComparer.OrdinalIgnoreCase)
                .Select(name => new Tag
                {
                    TagName = name,
                    CreateAt = DateTime.UtcNow,
                    LastEditTime = DateTime.UtcNow,
                    IsExist = true
                }).ToList();

            //只要NewTags不為空(=true)，把新的tag存到資料庫以獲取tagid
            if (NewTagsList.Any())
            {
                _context.Tags.AddRange(NewTagsList);
                await _context.SaveChangesAsync();
            }

            //把舊的和新的tag合併成一個list<tag>回傳
            return ExistingTags.Concat(NewTagsList).ToList();
        }

        //===處理ArticleList的summery===
        private string GetTextSummary(string htmlContent, int maxLength = 60)
        {
            if (string.IsNullOrEmpty(htmlContent))
                return "無內文...";

            // 用正規表達式把所有尖括號 <...> 裡面的東西換成空字串
            string cleanText = Regex.Replace(htmlContent, "<[^>]*>", "");

            // 去除前後多餘的空白
            cleanText = cleanText.Trim();

            // 如果洗乾淨的字串長度大於限制，就截斷並加上引號
            if (cleanText.Length > maxLength)
            {
                return cleanText.Substring(0, maxLength) + "...";
            }

            return cleanText;
        }

        //===處理文章的圖片===
        private List<string> ExtractImageUrlsFromContent(string? content)
        {
            if (string.IsNullOrWhiteSpace(content))
            {
                return new List<string>();
            }

            var matches = Regex.Matches(
                content,
                "<img[^>]+src=[\"'](?<src>[^\"']+)[\"'][^>]*>",
                RegexOptions.IgnoreCase
            );

            return matches
                .Select(m => m.Groups["src"].Value)
                .Where(url => !string.IsNullOrWhiteSpace(url))
                .Distinct()
                .ToList();
        }

        //===圖片同步===
        private async Task SyncArticleImagesAsync(int articleId, string? content)
        {
            var oldImages = await _context.ArticleImages
                .Where(img => img.ArticleId == articleId && img.IsExist == true)
                .ToListAsync();

            foreach (var img in oldImages)
            {
                img.IsExist = false;
                img.IsActive = false;
                img.LastEditTime = DateTime.UtcNow;
            }

            var imageUrls = ExtractImageUrlsFromContent(content);

            if (!imageUrls.Any())
            {
                return;
            }

            var newImages = imageUrls.Select((url, index) => new ArticleImage
            {
                ArticleId = articleId,
                ImageUrl = url,
                SortOrder = index + 1,
                CreateAt = DateTime.UtcNow,
                LastEditTime = DateTime.UtcNow,
                IsExist = true,
                IsActive = true
            }).ToList();

            _context.ArticleImages.AddRange(newImages);
        }
    }
}
