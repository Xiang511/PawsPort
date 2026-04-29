
using PawsPort.Dtos;
using PawsPort.Models;
using Microsoft.EntityFrameworkCore;

namespace PawsPort.Services
{
    public class ArticleService
    {
        private readonly PetDbContext _context;

        public ArticleService(PetDbContext context)
        {
            _context = context;
        }


        //文章列表



        //=====新增文章(非同步)=====
        public async Task<CreateArticleDTO> CreateArticleAsync(CreateArticleDTO articleDto)
        {
            //將ArticleDTO轉換為Article實體
            var ArticleEntity = new Article
            {
                CreateAt = DateTime.UtcNow,
                LastEditTime = DateTime.UtcNow,
                Title = articleDto.Title,
                Content = articleDto.Content,
                Status = articleDto.Status,
                ViewCount = 0,
                ReportedCount = 0,
                LastReported = null,
                EventStartDate = null,
                EventEndDate = null,
                EventLocation = null,
                IsExist = true,
                UserId = articleDto.UserId,
                CategoryId = articleDto.CategoryId,
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
            articleDto.ArticleId = ArticleEntity.ArticleId;

            return articleDto;
        }


        //=====修改文章(非同步)=====
        public async Task<UpdateArticleDTO> UpdateArticleAsync(UpdateArticleDTO articleDto)
        {
            //比對articleDto.articleId和資料庫裡的ArticleId，去資料庫撈出對應的文章實體
            var ArticleEntity = await _context.Articles.FirstOrDefaultAsync(a => a.ArticleId == articleDto.ArticleId);

            if (ArticleEntity == null)
            {
                //查無此文章，拋出例外或回傳null
                return null;
            }
            else
            {
                //找到文章實體
                //更新文章內容
                ArticleEntity.CategoryId = articleDto.CategoryId;

                ArticleEntity.LastEditTime = DateTime.UtcNow;
                ArticleEntity.Title = articleDto.Title;
                ArticleEntity.Content = articleDto.Content;
                ArticleEntity.Status = articleDto.Status;   //狀態(0:草稿,1:公開,2:封鎖)
                ArticleEntity.IsExist = articleDto.IsExist; //是否刪除

                ArticleEntity.EventStartDate = articleDto.EventStartDate;
                ArticleEntity.EventEndDate = articleDto.EventEndDate;
                ArticleEntity.EventLocation = articleDto.EventLocation;

                //先撈出原本的標籤關聯
                var ExistingMaps = await _context.ArticleTagMaps.Where(m => m.ArticleId == articleDto.ArticleId).ToListAsync();
                //刪除它
                _context.ArticleTagMaps.RemoveRange(ExistingMaps);
                //打包新的標籤list
                var NewTags = await PrepareTagsAsync(articleDto.TagNames);
                //建立新的標籤關聯(一個map = tagID + articleID)
                var NewMaps = NewTags.Select(t => new ArticleTagMap
                {
                    ArticleId = articleDto.ArticleId,
                    TagId = t.TagId

                }).ToList();
                //加入新的標籤關聯
                _context.ArticleTagMaps.AddRange(NewMaps);
                //存回資料庫
                await _context.SaveChangesAsync();
            }
            return articleDto;
        }



        //刪除文章

        //查詢文章

        //活動列表

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
