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

        public async Task<List<QaDTO>> GetAllQaAsync()
        {
            return await _db.QARecords
                .Select(q => new QaDTO
                {
                    Qaid = q.Qaid,
                    Csname = q.Csname,
                    ReplyContent = q.ReplyContent,
                    Note = q.Note,
                    ReplyDate = q.ReplyDate
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
                Csname = q.Csname,
                ReplyContent = q.ReplyContent,
                Note = q.Note,
                ReplyDate = q.ReplyDate
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
