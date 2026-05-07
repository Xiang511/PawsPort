using PawsPort.Models;
using PawsPort.Dtos;
using Microsoft.EntityFrameworkCore;

namespace PawsPort.Services
{
    public class MissingReportsService
    {
        private readonly PetDbContext _db;
        public MissingReportsService(PetDbContext db)
        {
            _db = db;
        }
        // 1. 取得搜尋列表
        public async Task<List<MissingReportListDto>> GetReportsAsync(string? keyword)
        {

            var query = from h in _db.MissingReports
                        join p in _db.Pets on h.PetId equals p.PetId
                        where p.DeletedAt == null
                        select new MissingReportListDto
                        {
                            ReportId = h.ReportId,
                            PetId = h.PetId,
                            Name = p.Name,
                            LastSeenDate = h.LastSeenDate,
                            IsActive = h.IsActive,
                            LastSeenLat = h.LastSeenLat,
                            LastSeenLng = h.LastSeenLng,
                            LostLocation = h.LostLocation,
                            UpdatedAt = h.UpdatedAt,
                            CreatedAt = h.CreatedAt,
                            UserId = h.UserId
                        };

            if (!string.IsNullOrEmpty(keyword))
            {
                query = query.Where(v => v.Name.Contains(keyword));
            }

            return await query.ToListAsync();
        }

        // 2. 新增報案
        public async Task CreateReportAsync(MissingReportCreateDto dto)
        {
            MissingReport report = new MissingReport
            {
                PetId = dto.PetId,
                LastSeenDate = dto.LastSeenDate,
                IsActive = dto.IsActive ?? true, // 預設為開啟
                LastSeenLat = dto.LastSeenLat,
                LastSeenLng = dto.LastSeenLng,
                LostLocation = dto.LostLocation,
                UserId = dto.UserId,
                CreatedAt = DateTime.Now
            };

            _db.MissingReports.Add(report);
            await _db.SaveChangesAsync();
        }

        // 3. 刪除報案
        public async Task DeleteReportAsync(int id)
        {
            
            var MissingReport = await _db.MissingReports.FirstOrDefaultAsync(p => p.ReportId == id);
            if (MissingReport != null)
            {
                MissingReport.DeletedAt = DateTime.Now;
                await _db.SaveChangesAsync();
            }
        }

        // 4. 取得單筆資料供編輯
        public async Task<MissingReportEditDto?> GetReportForEditAsync(int id)
        {
            
            var x = await _db.MissingReports.FirstOrDefaultAsync(p => p.ReportId == id);
            if (x == null) return null;

            return new MissingReportEditDto
            {
                ReportId = x.ReportId,
                LastSeenDate = x.LastSeenDate,
                LastSeenLat = x.LastSeenLat,
                LastSeenLng = x.LastSeenLng,
                IsActive = x.IsActive,
                LostLocation = x.LostLocation
            };
        }

        // 5. 更新報案資料
        public async Task UpdateReportAsync(MissingReportEditDto dto)
        {
            var dbReport = await _db.MissingReports.FirstOrDefaultAsync(p => p.ReportId == dto.ReportId);

            if (dbReport != null)
            {
                dbReport.LastSeenDate = dto.LastSeenDate;
                dbReport.LastSeenLat = dto.LastSeenLat;
                dbReport.LastSeenLng = dto.LastSeenLng;
                dbReport.IsActive = dto.IsActive;
                dbReport.LostLocation = dto.LostLocation;
                dbReport.UpdatedAt = DateTime.Now;

                await _db.SaveChangesAsync();
            }
        }
    }
}