using PawsPort.Models;
using PawsPort.Dtos;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using PawsPort.Helpers;

namespace PawsPort.Services
{
    public class PetAdoptionService
    {
        private readonly PetDbContext _db;
        private readonly IWebHostEnvironment _env;

        public PetAdoptionService(PetDbContext db, IWebHostEnvironment env)
        {
            _db = db;
            _env = env;
        }

        public async Task<List<PetAdoptionDTO>> GetAdoptionPetsAsync()
        {
            return await _db.Pets
                .Where(p => p.DeletedAt == null)
                .Select(p => new PetAdoptionDTO
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
                })
                .ToListAsync();
        }

        public async Task<PetAdoptionDTO> CreateAdoptionPetAsync(PetCreateDto dto)
        {
            var pet = new Pet
            {
                Species = dto.Species,
                Name = dto.Name,
                Gender = dto.Gender,
                Size = dto.Size,
                CoatColor = dto.CoatColor,
                BirthDate = dto.BirthDate,
                Photo = ImageUploadHelper.SaveBase64Image(dto.Photo, "pets", _env.WebRootPath),
                CurrentStatus = dto.CurrentStatus ?? 1, // 預設 1 表示開放領養/有效狀態
                BehavioralTraits = dto.BehavioralTraits,
                IsHighMaintenance = dto.IsHighMaintenance,
                Note = dto.Note,
                IsDesex = dto.IsDesex,
                Microchip = dto.Microchip,
                UserId = dto.UserId,
                CreatedAt = System.DateTime.Now
            };

            _db.Pets.Add(pet);
            await _db.SaveChangesAsync();

            return new PetAdoptionDTO
            {
                PetId = pet.PetId,
                Species = pet.Species,
                Name = pet.Name,
                Gender = pet.Gender,
                Size = pet.Size,
                CoatColor = pet.CoatColor,
                BirthDate = pet.BirthDate,
                Photo = pet.Photo,
                CurrentStatus = pet.CurrentStatus,
                BehavioralTraits = pet.BehavioralTraits,
                IsHighMaintenance = pet.IsHighMaintenance,
                Note = pet.Note,
                CreatedAt = pet.CreatedAt,
                IsDesex = pet.IsDesex,
                UpdatedAt = pet.UpdatedAt,
                DeletedAt = pet.DeletedAt,
                Microchip = pet.Microchip
            };
        }
    }
}
