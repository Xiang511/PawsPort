using Microsoft.EntityFrameworkCore;
using PawsPort.Dtos;
using PawsPort.Models;

namespace PawsPort.Services
{
    public class ENewsletterService
    {
        private readonly PetDbContext _db;

        public ENewsletterService(PetDbContext db)
        {
            _db = db;
        }


        public async Task<List<ENewsletterDTO>> GetAllNewslettersAsync()
        {
            var categoryOrder = new List<string>
            {
                "活動公告", "認養資訊", "飼養知識", "遊戲挑戰", "其它類別"
            };


            var listFromDb = await _db.ENewsletters
                .Where(n => n.Status != "已刪除")
                .ToListAsync();

            return listFromDb
                .OrderBy(n =>
                {
                    int index = categoryOrder.IndexOf(n.Category);
                    return index == -1 ? 99 : index; //防呆
                })
                .ThenByDescending(n => n.NewsLetterId)
                .Select(n => new ENewsletterDTO
                {
                    NewsLetterId = n.NewsLetterId,
                    Title = n.Title,
                    Summary = n.Summary,
                    Content = n.Content,
                    Category = n.Category,
                    Status = n.Status,
                    Note = n.Note,
                    PublishDate = n.PublishDate,
                    UserId = n.UserId
                })
                .ToList();
        }


        public async Task<ENewsletterDTO> CreateNewsletterAsync(ENewsletterCreateDTO dto)
        {
            var newNews = new ENewsletter
            {
                Title = dto.Title,
                Summary = dto.Summary,
                Content = dto.Content,
                Category = dto.Category,
                Status = dto.Status,
                Note = dto.Note,
                PublishDate = dto.PublishDate,
                //如果前端沒傳 UserId (null)，就給它 0，方便下面跑防呆
                UserId = dto.UserId ?? 0
            };


            //預設為草稿
            if (string.IsNullOrEmpty(newNews.Status))
            {
                newNews.Status = "草稿";
            }

            //自動填上現在時間
            if (newNews.Status == "已發送" && newNews.PublishDate == null)
            {
                newNews.PublishDate = DateTime.Now;
            }

            //預設UserId處理(接上登入功能前先給1)
            if (newNews.UserId == 0)
            {
                newNews.UserId = 1;
            }


            _db.ENewsletters.Add(newNews);
            await _db.SaveChangesAsync();


            return new ENewsletterDTO
            {
                NewsLetterId = newNews.NewsLetterId,
                Title = newNews.Title,
                Summary = newNews.Summary,
                Content = newNews.Content,
                Category = newNews.Category,
                Status = newNews.Status,
                Note = newNews.Note,
                PublishDate = newNews.PublishDate,
                UserId = newNews.UserId
            };
        }


        public async Task<bool> UpdateNewsletterAsync(int id, ENewsletterUpdateDTO dto)
        {
            var newsInDb = await _db.ENewsletters.FindAsync(id);

            //防呆：如果找不到，或者這筆資料已經被軟刪除，就不給改
            if (newsInDb == null || newsInDb.Status == "已刪除")
            {
                return false;
            }

            newsInDb.Title = dto.Title;
            newsInDb.Summary = dto.Summary;
            newsInDb.Content = dto.Content;
            newsInDb.Category = dto.Category;
            newsInDb.Note = dto.Note;
            newsInDb.Image = dto.Image; // 圖片也一起更新


            newsInDb.Status = string.IsNullOrEmpty(dto.Status) ? "草稿" : dto.Status;


            if (newsInDb.Status == "已發送" && dto.PublishDate == null)
            {
                newsInDb.PublishDate = DateTime.Now;
            }
            else
            {
                newsInDb.PublishDate = dto.PublishDate;
            }


            await _db.SaveChangesAsync();
            return true;
        }


        public async Task<bool> SoftDeleteNewsletterAsync(int id)
        {
            var news = await _db.ENewsletters.FindAsync(id);


            if (news == null || news.Status == "已刪除")
            {
                return false;
            }


            news.Status = "已刪除";
            await _db.SaveChangesAsync();
            return true;
        }
    }
}
