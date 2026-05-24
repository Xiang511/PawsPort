using Microsoft.EntityFrameworkCore;
using PawsPort.Dtos;
using PawsPort.Models;


namespace PawsPort.Services
{
    public class QaService
    {
        private readonly PetDbContext _db;

        public QaService(PetDbContext db)
        {
            _db = db;
        }

        public async Task<(List<QaDTO> Items, int TotalPages)> GetQaPagedAsync(int page, int pageSize = 10)
        {
            // 計算資料庫總共有幾筆資料
            var totalRecords = await _db.QARecords.CountAsync();

            // 計算總頁數 (無條件進位，例如 21 筆資料除以 10 = 3 頁)
            var totalPages = (int)Math.Ceiling((double)totalRecords / pageSize);

            var items = await _db.QARecords
                .OrderByDescending(q => q.QuestionDate)
                .Skip((page - 1) * pageSize)           // 跳過前面幾頁的資料
                .Take(pageSize)
                .Select(q => new QaDTO
                {
                    Qaid = q.Qaid,
                    UserId = q.UserId,
                    QuestionType = q.QuestionType,
                    ChiefComplaint = q.ChiefComplaint,
                    ChatContent = q.ChatContent,
                    Csname = q.Csname,
                    QuestionDate = q.QuestionDate,
                    ReplyContent = q.ReplyContent,
                    Note = q.Note,
                    ReplyDate = q.ReplyDate,
                    Score = q.Score
                })
                .ToListAsync();

            return (Data: items, TotalPages: totalPages);
        }


        // 取得單筆QA明細
        public async Task<QaDTO> GetQaByIdAsync(int id)
        {
            var q = await _db.QARecords.FindAsync(id);
            if (q == null) return null;

            return new QaDTO
            {
                Qaid = q.Qaid,
                UserId = q.UserId,
                QuestionType = q.QuestionType,
                ChiefComplaint = q.ChiefComplaint,
                ChatContent = q.ChatContent,
                Csname = q.Csname,
                QuestionDate = q.QuestionDate,
                ReplyContent = q.ReplyContent,
                Note = q.Note,
                ReplyDate = q.ReplyDate,
                Score = q.Score

            };
        }


        // 新增
        public async Task<bool> CreateQaAsync(QaCreateDTO dto)
        {
            var newQa = new QARecord
            {
                UserId = dto.UserId,
                QuestionType = dto.QuestionType,
                ChiefComplaint = dto.ChiefComplaint,
                ChatContent = dto.ChatContent,
                QuestionDate = DateTime.Now
            };

            _db.QARecords.Add(newQa);
            var result = await _db.SaveChangesAsync();

            // 如果成功寫入資料庫，會回傳影響的行數(大於0代表成功)
            return result > 0;
        }


        public async Task<bool> UpdateQaAsync(int id, QaUpdateDTO dto)
        {
            var qaData = await _db.QARecords.FindAsync(id);
            if (qaData == null) return false;


            qaData.Csname = dto.Csname;
            qaData.ReplyContent = dto.ReplyContent;
            qaData.Note = dto.Note;
            qaData.ReplyDate = DateTime.Now; //自動填入回覆當下的時間

            await _db.SaveChangesAsync();
            return true;
        }


        //圖表
        public async Task<QaDashboardDTO> GetDashboardDataAsync()
        {
            // 抓出最近30天的客服單
            var thirtyDaysAgo = DateTime.Now.AddDays(-30);
            var recentRecords = await _db.QARecords
                .Where(q => q.QuestionDate >= thirtyDaysAgo)
                .ToListAsync();

            var dashboardData = new QaDashboardDTO
            {
                QuestionTypeStats = recentRecords
                    .GroupBy(q => q.QuestionType)
                    .ToDictionary(
                        g => string.IsNullOrEmpty(g.Key) ? "未分類" : g.Key,
                        g => g.Count()
                    ),

                StatusStats = recentRecords
                    .GroupBy(q => q.Note)
                    .ToDictionary(

                        g => string.IsNullOrEmpty(g.Key) ? "未處理" : g.Key,
                        g => g.Count()
                    )
            };

            return dashboardData;


        }
    }
}
