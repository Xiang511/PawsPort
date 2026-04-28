using PawsPort.Models;
using PawsPort.Dtos;

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
        public List<PetListDto> GetPets(string? keyword)
        {


            // 基礎查詢：只抓沒被刪除的
            var query = _db.Pets.Where(p => p.DeletedAt == null);

            // 加上關鍵字搜尋
            if (!string.IsNullOrEmpty(keyword))
            {
                query = query.Where(p => p.Name.Contains(keyword) || p.CoatColor.Contains(keyword));
            }

            return query.Select(p => new PetListDto
            {
                PetId = p.PetId,
                Name = p.Name,
                CoatColor = p.CoatColor,
                Gender = p.Gender,
                Size = p.Size,
                CurrentStatus = p.CurrentStatus,
                CreatedAt = p.CreatedAt
            }).ToList();
        }

        // 2. 新增寵物
        public void CreatePet(PetCreateDto dto)
        {
            Pet pet = new Pet
            {
                SpeciesId = dto.SpeciesId,
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
            _db.SaveChanges();
        }

        // 3. 軟刪除邏輯 (不移除資料，僅標記時間)
        public void SoftDeletePet(int id)
        {
            var pet = _db.Pets.FirstOrDefault(p => p.PetId == id);

            if (pet != null)
            {
                pet.DeletedAt = DateTime.Now;
                _db.SaveChanges();
            }
        }

        // 4. 取得單筆資料供編輯
        public PetEditDto? GetPetForEdit(int id)
        {
            var p = _db.Pets.FirstOrDefault(x => x.PetId == id && x.DeletedAt == null);

            if (p == null) return null;

            return new PetEditDto
            {
                PetId = p.PetId,
                SpeciesId = p.SpeciesId,
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
        public void UpdatePet(PetEditDto dto)
        {
            var dbPet = _db.Pets.FirstOrDefault(p => p.PetId == dto.PetId);

            if (dbPet != null)
            {
                dbPet.SpeciesId = dto.SpeciesId;
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