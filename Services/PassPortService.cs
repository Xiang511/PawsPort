using PawsPort.Models;
using PawsPort.Dtos;
using System;
using System.Collections.Generic;
using System.Linq;

namespace PawsPort.Services
{
    public class PassPortService
    {
        // 1. 定義一個私有的唯讀欄位來存資料庫實體
        private readonly PetDbContext _db;

        // 2. 透過建構子注入：當系統建立這個 Service 時，會自動把已經註冊好的 DbContext 丟進來
        public PassPortService(PetDbContext db)
        {
            _db = db;
        }
        // 1. 取得列表
        public List<HealthPassportListDto> GetPassports(string? keyword)
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

            return query.ToList();
        }

        // 2. 新增護照 (包含病歷與疫苗)
        public void CreatePassport(HealthPassportCreateDto dto)
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
            _db.SaveChanges(); // 取得新的 PassportId

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

            _db.SaveChanges();
        }

        // 3. 刪除護照
        public void DeletePassport(int id)
        {
            
            var passport = _db.HealthPassports.FirstOrDefault(p => p.PassportId == id);

            if (passport != null)
            {
                passport.DeletedAt = DateTime.Now;
                _db.SaveChanges();
            }
        }

        // 4. 取得修改畫面需要的單筆資料
        public HealthPassportEditDto? GetPassportForEdit(int id)
        {
            
            HealthPassport x = _db.HealthPassports.FirstOrDefault(p => p.PassportId == id);

            if (x == null) return null;

            return new HealthPassportEditDto
            {
                PassportId = x.PassportId,
                Weight = x.Weight,
                Note = x.Note,
                RecordType = x.RecordType
            };
        }

        // 5. 儲存修改
        public void UpdatePassport(HealthPassportEditDto dto)
        {
            
            HealthPassport dbPassport = _db.HealthPassports.FirstOrDefault(p => p.PassportId == dto.PassportId);

            if (dbPassport != null)
            {
                dbPassport.Weight = dto.Weight;
                dbPassport.Note = dto.Note;
                dbPassport.RecordType = dto.RecordType;
                dbPassport.UpdatedAt = DateTime.Now;

                _db.SaveChanges();
            }
        }

        // 6. 取得詳細資料 (包含組裝防呆邏輯)
        public HealthPassportDetailsDto? GetPassportDetails(int id)
        {
            

            var m = _db.MedicalHistories.FirstOrDefault(x => x.PassportId == id);
            var v = _db.VaccinationStatuses.FirstOrDefault(x => x.PassportId == id);

            // 如果兩邊都沒資料，回傳 null 讓 Controller 決定怎麼處理 (例如發送錯誤訊息)
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