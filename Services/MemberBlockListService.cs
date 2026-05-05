using Microsoft.EntityFrameworkCore;
using PawsPort.Dtos;
using PawsPort.Models;
using PawsPort.ViewModels;
using Serilog;
using System.Data;
namespace PawsPort.Services
{
    public class MemberBlockListService
    {
        private readonly PetDbContext _context;

        public MemberBlockListService(PetDbContext context)
        {
            _context = context;
        }

        public async Task<List<MemberBlockListDTO>> GetBannedUsersAsync()
        {
            var bannedUsers = await _context.UserTables
                                 .Where(u => u.Status == false && u.DeleteDay == null)
                                 .Select(u => new MemberBlockListDTO
                                 {
                                     UserId = u.UserId,
                                     Name = u.Name,
                                     Status = u.Status,
                                     Note = u.Note,
                                     UpdatedAt = u.UpdatedAt
                                 }).ToListAsync();
        
            return bannedUsers;
        }

        public async Task<MemberBlockListDTO> CreateBannedUsersAsync(int? id , MemberBlockListEditDTO user)
        {
            var userEntity = await _context.UserTables.FirstOrDefaultAsync(u => u.UserId == id);

            if (userEntity == null)
            {
                throw new Exception("userEntity 為null");
            }
            userEntity.Status = user.Status;
            userEntity.Note = user.Note;
            userEntity.UpdatedAt = DateTime.Now;
            _context.UserTables.Update(userEntity);
            await _context.SaveChangesAsync();
            return new MemberBlockListDTO
            {
                UserId = userEntity.UserId,
                Name = userEntity.Name,
                Status = userEntity.Status,
                Note = userEntity.Note,
                UpdatedAt = userEntity.UpdatedAt
            };
        }


    }
}
