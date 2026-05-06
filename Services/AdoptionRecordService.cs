using PawsPort.Models;
using PawsPort.DTOs;
using Microsoft.EntityFrameworkCore;

namespace PawsPort.Services
{
    public class AdoptionRecordService
    {
        private readonly PetDbContext _db;
        public AdoptionRecordService(PetDbContext db)
        {
            _db = db;
        }
        // 1. 取得列表邏輯
        public async Task<List<AdoptionRecordListDto>> GetAdoptionRecordsAsync(string? keyword)
        { 

            var query = from h in _db.AdoptionRecords
                        join p in _db.Pets on h.PetId equals p.PetId
                        where p.DeletedAt == null
                        select new AdoptionRecordListDto
                        {
                            AdoptionId = h.AdoptionId,
                            PetId = h.PetId,
                            Name = p.Name,
                            UserId = h.UserId,
                            ApplyDate = h.ApplyDate,
                            AdoptDate = h.AdoptDate,
                            ReturnDate = h.ReturnDate,
                            ReturnReason = h.ReturnReason,
                            FollowUpDeadline = h.FollowUpDeadline,
                            Status = h.Status
                        };

            if (!string.IsNullOrEmpty(keyword))
            {
                query = query.Where(v => v.Name.Contains(keyword));
            }

            return await query.ToListAsync();
        }

        // 2. 新增邏輯
        public async Task CreateRecordAsync(AdoptionRecordCreateDto dto)
        { 

            // 將 DTO 轉換為真實的 Model (Entity) 以便存入資料庫
            AdoptionRecord newRecord = new AdoptionRecord
            {
                PetId = dto.PetId,
                UserId = dto.UserId,
                ApplyDate = dto.ApplyDate,
                AdoptDate = dto.AdoptDate,
                Status = dto.Status
            };

            _db.AdoptionRecords.Add(newRecord);
            await _db.SaveChangesAsync();
        }

        // 3. 刪除邏輯
        public async Task DeleteRecordAsync(int id)
        {
            
            AdoptionRecord AdoptRecord = await _db.AdoptionRecords.FirstOrDefaultAsync(p => p.AdoptionId == id);

            if (AdoptRecord != null)
            {
                AdoptRecord.DeletedAt = DateTime.Now;
                await _db.SaveChangesAsync();
            }
        }

        // 4. 根據 ID 取得單筆資料 (給 Edit 畫面用)
        public async Task<AdoptionRecordEditDto?> GetRecordForEditAsync(int id)
        {
            
            AdoptionRecord x = await _db.AdoptionRecords.FirstOrDefaultAsync(p => p.AdoptionId == id);

            if (x == null) return null;

            // 把 Model 轉成 DTO 傳出去
            return new AdoptionRecordEditDto
            {
                AdoptionId = x.AdoptionId,
                UserId = x.UserId,
                ApplyDate = x.ApplyDate,
                AdoptDate = x.AdoptDate,
                ReturnDate = x.ReturnDate,
                ReturnReason = x.ReturnReason,
                FollowUpDeadline = x.FollowUpDeadline,
                Status = x.Status
            };
        }

        // 5. 更新邏輯
        public async Task UpdateRecordAsync(AdoptionRecordEditDto dto)
        {
            
            AdoptionRecord dbAdoption = await _db.AdoptionRecords.FirstOrDefaultAsync(p => p.AdoptionId == dto.AdoptionId);

            if (dbAdoption != null)
            {
                // 用 DTO 的資料覆蓋資料庫的資料
                dbAdoption.ApplyDate = dto.ApplyDate;
                dbAdoption.AdoptDate = dto.AdoptDate;
                dbAdoption.ReturnDate = dto.ReturnDate;
                dbAdoption.ReturnReason = dto.ReturnReason;
                dbAdoption.FollowUpDeadline = dto.FollowUpDeadline;
                dbAdoption.UserId = dto.UserId;
                dbAdoption.Status = dto.Status;

                await _db.SaveChangesAsync();
            }
        }
    }
}