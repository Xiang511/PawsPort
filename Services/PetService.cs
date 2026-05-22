using PawsPort.Models;
using PawsPort.Dtos;
using Microsoft.EntityFrameworkCore;

namespace PawsPort.Services
{
    public class PetService
    {
        private readonly PetDbContext _db;
        public PetService(PetDbContext db)
        {
            _db = db;
        }
        // 1. 取得寵物列表 (含搜尋與軟刪除過濾)
        public async Task<List<PetListDto>> GetPetsAsync(string? keyword)
        {
            // 1. 這裡只是在「建立查詢語法」，還沒去資料庫，所以不可以加 await
            var query = _db.Pets.Where(p => p.DeletedAt == null);

            // 2. 繼續組合查詢條件 (一樣不加 await)
            if (!string.IsNullOrEmpty(keyword))
            {
                query = query.Where(p => p.Name.Contains(keyword) || p.CoatColor.Contains(keyword));
            }

            // 3. 執行階段：這才是真正去資料庫拿資料的時候
            // 必須在最前面加 await，最後面用 ToListAsync()
            return await query.Select(p => new PetListDto
            {
                PetId = p.PetId,
                Species = p.Species,
                Name = p.Name,
                Gender = p.Gender,
                Size = p.Size,
                CoatColor = p.CoatColor,
                BirthDate = p.BirthDate,
                Photo = p.Photo,
                CurrentStatus = p.CurrentStatus,
                BehavioralTraits = p.BehavioralTraits,
                IsHighMaintenance = p.IsHighMaintenance,
                Note = p.Note,
                CreatedAt = p.CreatedAt,
                IsDesex = p.IsDesex,
                UpdatedAt = p.UpdatedAt,
                DeletedAt = p.DeletedAt,
                Microchip = p.Microchip
            }).ToListAsync();
        }

        // 2. 新增寵物
        public async Task<int> CreatePetAsync(PetCreateDto dto)
        {
            Pet pet = new Pet
            {
                Species = dto.Species,
                Name = dto.Name,
                Gender = dto.Gender,
                Size = dto.Size,
                CoatColor = dto.CoatColor,
                CurrentStatus = dto.CurrentStatus,
                BehavioralTraits = dto.BehavioralTraits,
                IsHighMaintenance = dto.IsHighMaintenance,
                Note = dto.Note,
                IsDesex = dto.IsDesex,
                CreatedAt = DateTime.Now
            };

            _db.Pets.Add(pet);
            await _db.SaveChangesAsync();
            return pet.PetId;
        }

        // 3. 軟刪除邏輯 (不移除資料，僅標記時間)
        public async Task SoftDeletePetAsync(int id)
        {
            var pet = await _db.Pets.FirstOrDefaultAsync(p => p.PetId == id);

            if (pet != null)
            {
                pet.DeletedAt = DateTime.Now;
                await _db.SaveChangesAsync();
            }
        }

        // 4. 取得單筆資料供編輯
        public async Task<PetEditDto?> GetPetForEditAsync(int id)
        {
            var p = await _db.Pets.FirstOrDefaultAsync(x => x.PetId == id && x.DeletedAt == null);

            if (p == null) return null;

            return new PetEditDto
            {
                PetId = p.PetId,
                Species = p.Species,
                Name = p.Name,
                Gender = p.Gender,
                Size = p.Size,
                CoatColor = p.CoatColor,
                CurrentStatus = p.CurrentStatus,
                BehavioralTraits = p.BehavioralTraits,
                IsHighMaintenance = p.IsHighMaintenance,
                Note = p.Note,
                IsDesex = p.IsDesex
            };
        }

        // 5. 更新寵物資料
        public async Task UpdatePetAsync(PetEditDto dto)
        {
            var dbPet = await _db.Pets.FirstOrDefaultAsync(p => p.PetId == dto.PetId);

            if (dbPet != null)
            {
                dbPet.Species = dto.Species;
                dbPet.Name = dto.Name;
                dbPet.Gender = dto.Gender;
                dbPet.Size = dto.Size;
                dbPet.CoatColor = dto.CoatColor;
                dbPet.CurrentStatus = dto.CurrentStatus;
                dbPet.BehavioralTraits = dto.BehavioralTraits;
                dbPet.IsHighMaintenance = dto.IsHighMaintenance;
                dbPet.Note = dto.Note;
                dbPet.IsDesex = dto.IsDesex;
                dbPet.UpdatedAt = DateTime.Now;

                _db.SaveChanges();
            }
        }
    }
}