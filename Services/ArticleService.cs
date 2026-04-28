
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



        //新增文章(非同步)
        public async Task<CreateArticleDTO> CreateArticleAsync(CreateArticleDTO articleDto)
        {
            //將ArticleDTO轉換為Article實體
            var articleEntity = new Article
            {
                CreateAt = DateTime.UtcNow,
                LastEditTime = DateTime.UtcNow, 
                Title = articleDto.Title,
                Content = articleDto.Content,
                Status = articleDto.Status,
                ViewCount = 0,
                ReportedCount = 0,
                LastReported = null,
                EventStartDate = null,     //活動文章另外放在event，會固定categoryID
                EventEndDate = null,
                EventLocation = null,
                IsExist = true,
                UserId = articleDto.UserId,
                CategoryId = articleDto.CategoryId,
                DeleteTypeId = null,
                DeleteNote = null
            };

            //將Article實體添加到資料庫
            _context.Articles.Add(articleEntity);
            //保存更改到資料庫，為了拿到ArticleId
            await _context.SaveChangesAsync();

            //處理標籤
            var Tags = await PrepareTagsAsync(articleDto.TagNames);

            var Maps = Tags.Select(t => new ArticleTagMap
            {
                ArticleId = articleEntity.ArticleId,
                TagId = t.TagId,
            }).ToList();

            //將ArticleTagMap存入資料庫
            _context.ArticleTagMaps.AddRange(Maps);

            //存標籤關聯
            await _context.SaveChangesAsync();

            //將新增的Article實體轉換回ArticleDTO
            articleDto.ArticleId = articleEntity.ArticleId;
            
            return articleDto;
        }

        //修改文章

        //刪除文章

        //查詢文章

        //活動列表

        //處理標籤 //private

        private async Task<List<Tag>> PrepareTagsAsync(List<string> TagNames)
        {
            //先檢查有無TagNames
            if (TagNames == null || !TagNames.Any())
                return new List<Tag>();
            
            //去除空白和重複的標籤(不分大小寫)
            var UniqueTagNames = TagNames
                .Select(t=>t.Trim())
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
