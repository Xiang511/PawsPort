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

        public async Task<(List<QaDTO> Data, int TotalPages)> GetQaPagedAsync(int page, int pageSize = 10)
        {
            // 💡 2. 計算資料庫總共有幾筆資料
            var totalRecords = await _db.QARecords.CountAsync();

            // 💡 3. 計算總頁數 (無條件進位，例如 21 筆資料除以 10 = 3 頁)
            var totalPages = (int)Math.Ceiling((double)totalRecords / pageSize);

            var data = await _db.QARecords
                .OrderByDescending(q => q.QuestionDate)
                .Skip((page - 1) * pageSize)           // 跳過前面幾頁的資料
        .Take(pageSize)
                .Select(q => new QaDTO
                {
                    Qaid = q.Qaid,
                    UserId = q.UserId,
                    QuestionType = q.QuestionType,
                    ChiefComplaint = q.ChiefComplaint,
                    Csname = q.Csname,
                    QuestionDate = q.QuestionDate,
                    ReplyContent = q.ReplyContent,
                    Note = q.Note,
                    ReplyDate = q.ReplyDate,
                    Score = q.Score
                })
                .ToListAsync();
        }


        //取得單筆QA明細
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
                Csname = q.Csname,
                QuestionDate = q.QuestionDate,
                ReplyContent = q.ReplyContent,
                Note = q.Note,
                ReplyDate = q.ReplyDate,
                Score = q.Score

                // Qaid = q.Qaid,
                //Csname = q.Csname,
                //ReplyContent = q.ReplyContent,
                //Note = q.Note,
                //ReplyDate = q.ReplyDate
            };
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
    }
}
