using PawsPort.Models;
using PawsPort.Dtos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace PawsPort.Services
{
    public class PassPortService
    {
        private readonly PetDbContext _db;

        public PassPortService(PetDbContext db)
        {
            _db = db;
        }

        // 1. 取得列表 (非同步)
        public async Task<List<HealthPassportListDto>> GetPassportsAsync(string? keyword)
        {
            var query = from h in _db.HealthPassports
                        join p in _db.Pets on h.PetId equals p.PetId
                        where p.DeletedAt == null
                        select new HealthPassportListDto
                        {
                            PassportId = h.PassportId,
                            PetId = h.PetId,
                            Name = p.Name,
                            Weight = h.Weight,
                            Note = h.Note,
                            RecordType = h.RecordType,
                            RecordDate = h.RecordDate,
                            UpdatedAt = h.UpdatedAt,
                            CreatedAt = h.CreatedAt
                        };

            if (!string.IsNullOrEmpty(keyword))
            {
                query = query.Where(v => v.Name.Contains(keyword));
            }

            // 使用 await 與 ToListAsync()
            return await query.ToListAsync();
        }

        // 2. 新增護照 (非同步)
        public async Task CreatePassportAsync(HealthPassportCreateDto dto)
        {
            // 步驟 1：主表
            HealthPassport passport = new HealthPassport
            {
                PetId = dto.PetId,
                Weight = dto.Weight,
                Note = dto.Note,
                RecordType = dto.RecordType,
                RecordDate = dto.RecordDate,
                CreatedAt = DateTime.Now
            };
            _db.HealthPassports.Add(passport);

            // 使用 await 與 SaveChangesAsync()
            await _db.SaveChangesAsync();

            // 步驟 2：病歷表
            if (!string.IsNullOrEmpty(dto.Disease))
            {
                MedicalHistory medical = new MedicalHistory
                {
                    PassportId = passport.PassportId,
                    Location = dto.TreatmentLocation,
                    Disease = dto.Disease,
                    DiseaseTreatment = dto.DiseaseTreatment,
                    Time = dto.TreatmentTime,
                    CreatedAt = DateTime.Now
                };
                _db.MedicalHistories.Add(medical);
            }

            // 步驟 3：疫苗表
            if (!string.IsNullOrEmpty(dto.Type))
            {
                VaccinationStatus vaccine = new VaccinationStatus
                {
                    PassportId = passport.PassportId,
                    Type = dto.Type,
                    Location = dto.VaccinationLocation,
                    Time = dto.VaccinationTime,
                    Forecast = dto.Forecast,
                    CreatedAt = DateTime.Now
                };
                _db.VaccinationStatuses.Add(vaccine);
            }

            await _db.SaveChangesAsync();
        }

        // 3. 刪除護照 (非同步)
        public async Task DeletePassportAsync(int id)
        {
            // 使用 FirstOrDefaultAsync
            var passport = await _db.HealthPassports.FirstOrDefaultAsync(p => p.PassportId == id);

            if (passport != null)
            {
                passport.DeletedAt = DateTime.Now;
                await _db.SaveChangesAsync();
            }
        }

        // 4. 取得修改畫面需要的單筆資料 (非同步)
        public async Task<HealthPassportEditDto?> GetPassportForEditAsync(int id)
        {
            // 使用 FirstOrDefaultAsync
            HealthPassport? x = await _db.HealthPassports.FirstOrDefaultAsync(p => p.PassportId == id);

            if (x == null) return null;

            return new HealthPassportEditDto
            {
                PassportId = x.PassportId,
                Weight = x.Weight,
                Note = x.Note,
                RecordType = x.RecordType
            };
        }

        // 5. 儲存修改 (非同步)
        public async Task UpdatePassportAsync(HealthPassportEditDto dto)
        {
            var dbPassport = await _db.HealthPassports.FirstOrDefaultAsync(p => p.PassportId == dto.PassportId);

            if (dbPassport != null)
            {
                dbPassport.Weight = dto.Weight;
                dbPassport.Note = dto.Note;
                dbPassport.RecordType = dto.RecordType;
                dbPassport.UpdatedAt = DateTime.Now;

                await _db.SaveChangesAsync();
            }
        }

        // 6. 取得詳細資料 (非同步)
        public async Task<HealthPassportDetailsDto?> GetPassportDetailsAsync(int id)
        {
            // 這裡可以嘗試同時發出兩個查詢來優化效能
            // 正確寫法：依序等待 (Sequential Await)
            var m = await _db.MedicalHistories.FirstOrDefaultAsync(x => x.PassportId == id);
            var v = await _db.VaccinationStatuses.FirstOrDefaultAsync(x => x.PassportId == id);

            if (m == null && v == null) return null;

            return new HealthPassportDetailsDto
            {
                PassportId = id,
                MedicalDetailId = m?.MedicalDetailId,
                TreatmentLocation = m?.Location ?? "尚未有資料",
                Disease = m?.Disease ?? "尚未有資料",
                DiseaseTreatment = m?.DiseaseTreatment ?? "尚未有資料",
                TreatmentTime = m?.Time,

                HistoryId = v?.HistoryId,
                Type = v?.Type ?? "尚未有資料",
                VaccinationLocation = v?.Location ?? "尚未有資料",
                VaccinationTime = v?.Time
            };
        }
    }
}