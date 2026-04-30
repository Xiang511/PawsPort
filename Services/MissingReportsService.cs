using PawsPort.Models;
using PawsPort.Dtos;

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
        public List<MissingReportListDto> GetReports(string? keyword)
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

            return query.ToList();
        }

        // 2. 新增報案
        public void CreateReport(MissingReportCreateDto dto)
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
            _db.SaveChanges();
        }

        // 3. 刪除報案
        public void DeleteReport(int id)
        {
            
            var missingreport = _db.MissingReports.FirstOrDefault(p => p.ReportId == id);
            if (missingreport != null)
            {
                missingreport.DeletedAt = DateTime.Now;
                _db.SaveChanges();
            }
        }

        // 4. 取得單筆資料供編輯
        public MissingReportEditDto? GetReportForEdit(int id)
        {
            
            var x = _db.MissingReports.FirstOrDefault(p => p.ReportId == id);
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
        public void UpdateReport(MissingReportEditDto dto)
        {
            var dbReport = _db.MissingReports.FirstOrDefault(p => p.ReportId == dto.ReportId);

            if (dbReport != null)
            {
                dbReport.LastSeenDate = dto.LastSeenDate;
                dbReport.LastSeenLat = dto.LastSeenLat;
                dbReport.LastSeenLng = dto.LastSeenLng;
                dbReport.IsActive = dto.IsActive;
                dbReport.LostLocation = dto.LostLocation;
                dbReport.UpdatedAt = DateTime.Now;

                _db.SaveChanges();
            }
        }
    }
}