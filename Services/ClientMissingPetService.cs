using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using PawsPort.Dtos;
using PawsPort.Models;
using Microsoft.AspNetCore.Hosting;
using PawsPort.Helpers;

namespace PawsPort.Services
{
    public class ClientMissingPetService
    {
        private readonly PetDbContext _context;
        private readonly IWebHostEnvironment _env;

        public ClientMissingPetService(PetDbContext context, IWebHostEnvironment env)
        {
            _context = context;
            _env = env;
        }

        // 解析 Note 欄位取得 Color 與 ChipId
        private (string Color, string ChipId) ParseNote(string note)
        {
            if (string.IsNullOrWhiteSpace(note)) return ("未知", "無");

            try
            {
                // 嘗試當作 JSON 解析
                var dict = JsonSerializer.Deserialize<Dictionary<string, string>>(note);
                string color = dict != null && dict.ContainsKey("Color") ? dict["Color"] : "未知";
                string chipId = dict != null && dict.ContainsKey("ChipId") ? dict["ChipId"] : "無";
                return (color, chipId);
            }
            catch
            {
                // 如果不是 JSON 格式，則直接回傳原字串或預設值
                return ("未知", "無");
            }
        }

        public async Task<List<MissingPetListDTO>> GetMissingPetsAsync()
        {
            var reports = await _context.MissingReports
                .Where(r => r.DeletedAt == null && r.IsActive != false)
                .OrderByDescending(r => r.CreatedAt)
                .ToListAsync();

            var dtos = reports.Select(r => 
            {
                var (color, chipId) = ParseNote(r.Note);
                return new MissingPetListDTO
                {
                    Id = r.ReportId,
                    Breed = r.Name ?? "未知",
                    Gender = r.Gender ?? "未知",
                    City = r.LostCity ?? "",
                    District = r.LostDistrict ?? "",
                    LostTime = r.LastSeenDate?.ToString("yyyy-MM-dd") ?? "",
                    LostPlace = r.LostLocation ?? "",
                    ChipId = chipId,
                    Feature = r.Features ?? "",
                    Photo = string.IsNullOrEmpty(r.Photo) ? "https://images.unsplash.com/photo-1583511655857-d19b40a7a54e?auto=format&fit=crop&q=80&w=400" : r.Photo,
                    Color = color
                };
            }).ToList();

            return dtos;
        }

        public async Task<MissingPetDetailDTO> GetMissingPetDetailAsync(int id)
        {
            var r = await _context.MissingReports
                .FirstOrDefaultAsync(m => m.ReportId == id && m.DeletedAt == null);

            if (r == null) return null;

            var (color, chipId) = ParseNote(r.Note);

            // 嘗試取得報案人的名字
            string reporterName = "熱心民眾";
            if (r.UserId.HasValue)
            {
                var user = await _context.UserTables.FirstOrDefaultAsync(u => u.UserId == r.UserId.Value);
                if (user != null && !string.IsNullOrWhiteSpace(user.Name)) reporterName = user.Name;
            }

            return new MissingPetDetailDTO
            {
                Id = r.ReportId,
                Breed = r.Name ?? "未知",
                Gender = r.Gender ?? "未知",
                City = r.LostCity ?? "",
                District = r.LostDistrict ?? "",
                LostTime = r.LastSeenDate?.ToString("yyyy-MM-dd") ?? "",
                LostPlace = r.LostLocation ?? "",
                ChipId = chipId,
                Feature = r.Features ?? "",
                Photo = string.IsNullOrEmpty(r.Photo) ? "https://images.unsplash.com/photo-1583511655857-d19b40a7a54e?auto=format&fit=crop&q=80&w=400" : r.Photo,
                Color = color,
                Species = r.Species ?? "狗",
                ReporterName = reporterName,
                ContactPhone = r.ContactPhone ?? "",
                ContactEmail = r.ContactEmail ?? "",
                AddDate = r.CreatedAt?.ToString("yyyy-MM-dd") ?? ""
            };
        }

        public async Task<MissingPetDetailDTO> CreateMissingPetAsync(CreateMissingPetDTO dto, int userId)
        {
            // 將毛色與晶片號碼封裝成 JSON 存入 Note
            var noteData = new Dictionary<string, string>
            {
                { "Color", string.IsNullOrWhiteSpace(dto.FurColor) ? "未知" : dto.FurColor },
                { "ChipId", string.IsNullOrWhiteSpace(dto.HasTag) ? "無" : dto.HasTag }
            };
            string noteJson = JsonSerializer.Serialize(noteData);

            DateTime? parsedLostDate = null;
            if (DateTime.TryParse(dto.LostDate, out DateTime dt))
            {
                parsedLostDate = dt;
            }

            var report = new MissingReport
            {
                UserId = userId,
                Name = dto.PetName,
                Species = dto.PetType,
                Gender = dto.Gender ?? "未知",
                LastSeenDate = parsedLostDate,
                LostCity = dto.City,
                LostDistrict = dto.District,
                LostLocation = dto.Address,
                Features = dto.Features,
                ContactPhone = dto.ContactPhone,
                ContactEmail = dto.ContactEmail,
                Photo = ImageUploadHelper.SaveBase64Image(dto.Photo, "missingreports", _env.WebRootPath),
                Note = noteJson,
                IsActive = true,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            _context.MissingReports.Add(report);
            await _context.SaveChangesAsync();

            return await GetMissingPetDetailAsync(report.ReportId);
        }
    }
}
