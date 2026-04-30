
using PawsPort.Dtos;
using PawsPort.Models;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;

namespace PawsPort.Services
{
    public class ArticleService
    {
        private readonly PetDbContext _context;

        public ArticleService(PetDbContext context)
        {
            _context = context;
        }



        //=====新增文章(非同步)=====
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


        //=====修改文章(非同步)=====
        public async Task<int?> UpdateArticleAsync(int id, ArticleSaveDTO articleDto)
        {
            //比對articleDto.articleId和資料庫裡的ArticleId，去資料庫撈出對應的文章實體
            var ArticleEntity = await _context.Articles.FirstOrDefaultAsync(a => a.ArticleId == id);

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


        //=====刪除文章(非同步)=====
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
            var ArticleList = await query.Select(x => new ArticleListDTO
            {
                ArticleId = x.a.ArticleId,
                Title = x.a.Title,
                //Summary = x.a.Content.Length > 100 ? x.a.Content.Substring(0, 100) + "..." : x.a.Content,
                CreateAt = x.a.CreateAt,
                LastEditTime = x.a.LastEditTime,
                Status = x.a.Status,
                ViewCount = x.a.ViewCount,
                EventStartDate = x.a.EventStartDate,
                EventEndDate = x.a.EventEndDate,
                EventLocation = x.a.EventLocation,
                CategoryName = x.c.CategoryName,
                UserName = x.u.Name
            }).ToListAsync();

            //從文章列表中撈出文章id
            var ArticleIds = ArticleList.Select(a => a.ArticleId).ToList();

            //撈出對應的tagid
            var Tags = await (from m in _context.ArticleTagMaps
                              join t in _context.Tags on m.TagId equals t.TagId
                              where ArticleIds.Contains(m.ArticleId) && m.IsExist == true
                              select new
                              {
                                  m.ArticleId,
                                  t.TagName
                              }).ToListAsync();

            //把tagname塞回對應的文章裡
            foreach (var a in ArticleList)
            {
                a.TagNames = Tags.Where(t => t.ArticleId == a.ArticleId).Select(t => t.TagName).ToList();
            }
            return ArticleList;
        }


        //=====查詢文章(文章id)=====

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
                .Select(t => t.Trim())
                .Distinct(StringComparer.OrdinalIgnoreCase);

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

    }
}
