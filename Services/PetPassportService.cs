using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using PawsPort.Dtos;
using PawsPort.Models;
using Microsoft.AspNetCore.Hosting;
using PawsPort.Helpers;

namespace PawsPort.Services
{
    public class PetPassportService
    {
        private readonly PetDbContext _context;
        private readonly IWebHostEnvironment _env;

        public PetPassportService(PetDbContext context, IWebHostEnvironment env)
        {
            _context = context;
            _env = env;
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
                var medicalEntities = await _context.MedicalHistories
                .Where(m => m.PassportId == hp.PassportId)
                .ToListAsync();

                var medicals = medicalEntities.Select(m => new MedicalRecordDto
                {
                    Disease = m.Disease,
                    DiseaseTreatment = m.DiseaseTreatment,
                    Location = m.Location,
                    Time = m.Time.HasValue ? m.Time.Value.ToString("yyyy-MM-dd") : ""
                }).ToList();

                // 取得疫苗紀錄
                var vaccineEntities = await _context.VaccinationStatuses
                 .Where(v => v.PassportId == hp.PassportId)
                 .ToListAsync();

                var vaccines = vaccineEntities.Select(v => new VaccinationDto
                {
                    Type = v.Type,
                    Location = v.Location,
                    Time = v.Time.HasValue ? v.Time.Value.ToString("yyyy-MM-dd") : "",
                    Forecast = v.Forecast.HasValue ? v.Forecast.Value.ToString("yyyy-MM-dd") : ""
                }).ToList();

                // 取得該寵物歷史所有體重趨勢紀錄
                var weightEntities = await _context.HealthPassports
                .Where(h => h.PetId == hp.PetId && h.DeletedAt == null && h.Weight.HasValue)
                .OrderBy(h => h.RecordDate)
                .ToListAsync();

                // 2. 在記憶體中進行 Select，這時候就能用 index 和 ToString 了
                var weights = weightEntities.Select((h, index) => new WeightRecordDto
                {
                    Id = index + 1,
                    Date = h.RecordDate.HasValue ? h.RecordDate.Value.ToString("yyyy-MM-dd") : "",
                    Weight = h.Weight.Value
                }).ToList();
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
            if (!string.IsNullOrEmpty(dto.Photo)) hp.Photo = ImageUploadHelper.SaveBase64Image(dto.Photo, "passports", _env.WebRootPath);
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

        public async Task<PetPassportDetailDto> CreatePassportAsync(PetPassportUpsertDto dto, int userId)
        {
            int petId = dto.PetId ?? 0;

            // 如果沒有傳入 PetId，或是 PetId 為 0，則自動為該使用者建立一隻新的寵物
            if (petId == 0)
            {
                var newPet = new Pet
                {
                    Name = dto.Name ?? "新毛孩",
                    Gender = dto.Gender,
                    IsDesex = dto.IsDesex,
                    BirthDate = dto.BirthDate,
                    Photo = ImageUploadHelper.SaveBase64Image(dto.Photo, "pets", _env.WebRootPath) ?? "default_pet.jpg",
                    UserId = userId,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };
                _context.Pets.Add(newPet);
                await _context.SaveChangesAsync();
                petId = newPet.PetId;
            }
            else
            {
                // 如果傳入已有的 PetId，更新該寵物的健康狀態與基本資料
                var pet = await _context.Pets.FirstOrDefaultAsync(p => p.PetId == petId);
                if (pet != null)
                {
                    if (!string.IsNullOrEmpty(dto.Name)) pet.Name = dto.Name;
                    if (dto.Gender.HasValue) pet.Gender = dto.Gender;
                    pet.IsDesex = dto.IsDesex;
                    if (dto.BirthDate.HasValue) pet.BirthDate = dto.BirthDate;
                    if (!string.IsNullOrEmpty(dto.Photo)) pet.Photo = ImageUploadHelper.SaveBase64Image(dto.Photo, "pets", _env.WebRootPath);
                    pet.UpdatedAt = DateTime.UtcNow;
                    _context.Pets.Update(pet);
                }
            }

            var hp = new HealthPassport
            {
                PetId = petId,
                UserId = userId,
                RecordDate = dto.RecordDate ?? DateOnly.FromDateTime(DateTime.Today),
                Weight = dto.Weight,
                Note = dto.Note,
                Photo = ImageUploadHelper.SaveBase64Image(dto.Photo, "passports", _env.WebRootPath) ?? "default_pet.jpg",
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
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

        // 任務 4：取得合併的健康護照資料 (Unified GET)
        public async Task<List<UnifiedPassportDTO>> GetUnifiedAsync(int userId)
        {
            var passports = await _context.HealthPassports
                .Where(hp => hp.UserId == userId && hp.DeletedAt == null)
                .ToListAsync();

            var result = new List<UnifiedPassportDTO>();

            foreach (var hp in passports)
            {
                var pet = await _context.Pets.FirstOrDefaultAsync(p => p.PetId == hp.PetId);
                
                // 取得醫療史
                var medicalEntities = await _context.MedicalHistories
                    .Where(m => m.PassportId == hp.PassportId)
                    .ToListAsync();

                var medicals = medicalEntities.Select(m => new MedicalDto
                {
                    MedicalDetailId = m.MedicalDetailId,
                    Location = m.Location,
                    Disease = m.Disease,
                    DiseaseTreatment = m.DiseaseTreatment,
                    Time = m.Time
                }).ToList();

                // 取得疫苗紀錄
                var vaccineEntities = await _context.VaccinationStatuses
                    .Where(v => v.PassportId == hp.PassportId)
                    .ToListAsync();

                var vaccines = vaccineEntities.Select(v => new VaccineDto
                {
                    HistoryId = v.HistoryId,
                    Type = v.Type,
                    Location = v.Location,
                    Time = v.Time,
                    Forecast = v.Forecast
                }).ToList();

                result.Add(new UnifiedPassportDTO
                {
                    PassportId = hp.PassportId,
                    PetId = hp.PetId,
                    PetName = pet?.Name ?? "未知毛孩",
                    Gender = pet?.Gender == 1 ? "公" : pet?.Gender == 2 ? "母" : "未知",
                    Age = pet != null ? CalculateAge(pet.BirthDate) : "未知年齡",
                    Weight = hp.Weight,
                    RecordDate = hp.RecordDate,
                    Note = hp.Note,
                    PhotoBase64 = hp.Photo,
                    MedicalHistories = medicals,
                    VaccinationStatuses = vaccines
                });
            }

            return result;
        }

        // 任務 5：新增或更新合併的護照細節 (Unified POST)
        public async Task UpsertUnifiedAsync(CreateOrUpdatePassportDTO dto, int userId)
        {
            int? passportId = dto.PassportId;
            if (!passportId.HasValue || passportId == 0)
            {
                var passport = await _context.HealthPassports
                    .FirstOrDefaultAsync(hp => hp.UserId == userId && hp.DeletedAt == null);
                if (passport == null)
                {
                    throw new Exception("找不到該使用者的寵物健康護照紀錄");
                }
                passportId = passport.PassportId;
            }

            if (dto.DetailType == "medical")
            {
                var medical = new MedicalHistory
                {
                    PassportId = passportId,
                    Location = dto.Location,
                    Disease = dto.Disease,
                    DiseaseTreatment = dto.DiseaseTreatment,
                    Time = dto.Time,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };
                _context.MedicalHistories.Add(medical);
            }
            else if (dto.DetailType == "vaccine")
            {
                var vaccine = new VaccinationStatus
                {
                    PassportId = passportId,
                    Type = dto.VaccineType,
                    Location = dto.VaccineLocation,
                    Time = dto.VaccineTime,
                    Forecast = dto.Forecast,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };
                _context.VaccinationStatuses.Add(vaccine);
            }
            else if (dto.DetailType == "image")
            {
                var passport = await _context.HealthPassports
                    .FirstOrDefaultAsync(hp => hp.PassportId == passportId && hp.DeletedAt == null);
                if (passport != null)
                {
                    passport.Photo = ImageUploadHelper.SaveBase64Image(dto.PhotoBase64, "passports", _env.WebRootPath);
                    if (!string.IsNullOrEmpty(dto.PhotoNote))
                    {
                        passport.Note = dto.PhotoNote;
                    }
                    passport.UpdatedAt = DateTime.UtcNow;
                    _context.HealthPassports.Update(passport);
                }
            }
            else if (dto.DetailType == "weight")
            {
                var mainPassport = await _context.HealthPassports
                    .FirstOrDefaultAsync(hp => hp.PassportId == passportId && hp.DeletedAt == null);
                if (mainPassport != null)
                {
                    // 如果原本的主護照已經有體重，且與新輸入的日期不同，則將舊的體重備份為一筆獨立的歷史紀錄
                    if (mainPassport.Weight.HasValue && mainPassport.RecordDate.HasValue && mainPassport.RecordDate != dto.RecordDate)
                    {
                        var historyWeight = new HealthPassport
                        {
                            PetId = mainPassport.PetId,
                            UserId = null, // 留空，避免產生重複的寵物護照主卡片
                            RecordDate = mainPassport.RecordDate.Value,
                            Weight = mainPassport.Weight.Value,
                            Note = mainPassport.Note,
                            CreatedAt = mainPassport.UpdatedAt ?? mainPassport.CreatedAt ?? DateTime.UtcNow,
                            UpdatedAt = DateTime.UtcNow
                        };
                        _context.HealthPassports.Add(historyWeight);
                    }

                    // 更新主護照的最新體重資訊，這樣在護照清單首頁能直接看到最新的體重
                    if (dto.Weight.HasValue) mainPassport.Weight = dto.Weight;
                    if (dto.RecordDate.HasValue) mainPassport.RecordDate = dto.RecordDate;
                    if (!string.IsNullOrEmpty(dto.Note)) mainPassport.Note = dto.Note;
                    mainPassport.UpdatedAt = DateTime.UtcNow;
                    _context.HealthPassports.Update(mainPassport);
                }
            }

            await _context.SaveChangesAsync();
        }
    }
}
