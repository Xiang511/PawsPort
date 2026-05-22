using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using PawsPort.Dtos;
using PawsPort.Models;

namespace PawsPort.Services
{
    public class PetPassportService
    {
        private readonly PetDbContext _context;

        public PetPassportService(PetDbContext context)
        {
            _context = context;
        }

        // 任務 1：取得使用者的所有護照清單
        public async Task<List<PetPassportDisplayDto>> GetPetPassportsAsync(int userId)
        {
            // 找出該使用者所有未軟刪除的健康護照紀錄
            var passports = await _context.HealthPassports
                .Where(hp => hp.UserId == userId && hp.DeletedAt == null)
                .ToListAsync();

            var resultList = new List<PetPassportDisplayDto>();

            foreach (var hp in passports)
            {
                // 關聯 Pet 基本資訊
                var pet = await _context.Pets.FirstOrDefaultAsync(p => p.PetId == hp.PetId);
                if (pet == null) continue;

                // 計算年齡
                string ageStr = CalculateAge(pet.BirthDate);

                // 取得醫療史
                var medicals = await _context.MedicalHistories
                    .Where(m => m.PassportId == hp.PassportId)
                    .Select(m => new MedicalRecordDto
                    {
                        Disease = m.Disease,
                        DiseaseTreatment = m.DiseaseTreatment,
                        Location = m.Location,
                        Time = m.Time.HasValue ? m.Time.Value.ToString("yyyy-MM-dd") : ""
                    }).ToListAsync();

                // 取得疫苗紀錄
                var vaccines = await _context.VaccinationStatuses
                    .Where(v => v.PassportId == hp.PassportId)
                    .Select(v => new VaccinationDto
                    {
                        Type = v.Type,
                        Location = v.Location,
                        Time = v.Time.HasValue ? v.Time.Value.ToString("yyyy-MM-dd") : "",
                        Forecast = v.Forecast.HasValue ? v.Forecast.Value.ToString("yyyy-MM-dd") : ""
                    }).ToListAsync();

                // 取得該寵物歷史所有體重趨勢紀錄
                var weights = await _context.HealthPassports
                    .Where(h => h.PetId == hp.PetId && h.DeletedAt == null && h.Weight.HasValue)
                    .OrderBy(h => h.RecordDate)
                    .Select((h, index) => new WeightRecordDto
                    {
                        Id = index + 1,
                        Date = h.RecordDate.HasValue ? h.RecordDate.Value.ToString("yyyy-MM-dd") : "",
                        Weight = h.Weight.Value
                    }).ToListAsync();

                resultList.Add(new PetPassportDisplayDto
                {
                    Id = hp.PassportId,
                    Name = pet.Name,
                    Age = ageStr,
                    Gender = pet.Gender,
                    IsDesex = pet.IsDesex,
                    Photo = hp.Photo ?? "default_pet.jpg",
                    Weight = hp.Weight,
                    MedicalRecords = medicals,
                    Vaccinations = vaccines,
                    WeightRecords = weights
                });
            }

            return resultList;
        }

        // 任務 2：取得單筆護照資料
        public async Task<PetPassportDetailDto> GetPassportDetailAsync(int passportId, int userId)
        {
            var hp = await _context.HealthPassports
                .FirstOrDefaultAsync(h => h.PassportId == passportId && h.UserId == userId && h.DeletedAt == null);

            if (hp == null) return null;

            var pet = await _context.Pets.FirstOrDefaultAsync(p => p.PetId == hp.PetId);

            return new PetPassportDetailDto
            {
                Id = hp.PassportId,
                Name = pet?.Name ?? "未知毛孩",
                Gender = pet?.Gender,
                BirthDate = pet?.BirthDate,
                IsDesex = pet?.IsDesex ?? false,
                RecordDate = hp.RecordDate.HasValue ? hp.RecordDate.Value.ToString("yyyy-MM-dd") : "",
                Weight = hp.Weight,
                Note = hp.Note,
                Photo = hp.Photo
            };
        }

        // 任務 2：更新護照資料
        public async Task<bool> UpdatePassportAsync(int passportId, PetPassportUpsertDto dto, int userId)
        {
            var hp = await _context.HealthPassports
                .FirstOrDefaultAsync(h => h.PassportId == passportId && h.UserId == userId && h.DeletedAt == null);

            if (hp == null) return false;

            // 更新護照主表
            hp.RecordDate = dto.RecordDate;
            hp.Weight = dto.Weight;
            hp.Note = dto.Note;
            if (!string.IsNullOrEmpty(dto.Photo)) hp.Photo = dto.Photo;
            hp.UpdatedAt = DateTime.UtcNow;

            // 聯動更新 Pet 資料表部分健康屬性
            var pet = await _context.Pets.FirstOrDefaultAsync(p => p.PetId == hp.PetId);
            if (pet != null)
            {
                pet.Gender = dto.Gender;
                pet.IsDesex = dto.IsDesex;
                _context.Pets.Update(pet);
            }

            _context.HealthPassports.Update(hp);
            await _context.SaveChangesAsync();
            return true;
        }

        // 任務 3：新增護照資料
        public async Task<PetPassportDetailDto> CreatePassportAsync(PetPassportUpsertDto dto, int userId)
        {
            var hp = new HealthPassport
            {
                PetId = dto.PetId.Value, // 這裡假設傳入時 PetId 一定有值
                UserId = userId,
               
                RecordDate = dto.RecordDate,
                Weight = dto.Weight,
                Note = dto.Note,
                Photo = dto.Photo
                // 若資料庫有其他必填欄位 (如 CreatedAt) 請在此一併補上
            };
            _context.HealthPassports.Add(hp);
            await _context.SaveChangesAsync();

            return await GetPassportDetailAsync(hp.PassportId, userId);
        }

        // 輔助方法：年齡計算邏輯
        private string CalculateAge(DateOnly? birthDate) // 修改參數型別為 DateOnly?
        {
            if (!birthDate.HasValue) return "未知年齡";

            // 取得今天的 DateOnly
            DateOnly today = DateOnly.FromDateTime(DateTime.Today);

            int years = today.Year - birthDate.Value.Year;
            int months = today.Month - birthDate.Value.Month;

            // 如果今天的日子比生日早，表示還沒滿那個月
            if (today.Day < birthDate.Value.Day) months--;

            if (months < 0)
            {
                years--;
                months += 12;
            }

            if (years <= 0) return $"{months}個月";
            return months == 0 ? $"{years}歲" : $"{years}歲{months}個月";
        }
    }
}