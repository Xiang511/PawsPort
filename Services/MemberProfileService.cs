using Microsoft.AspNetCore.Http.HttpResults;
using PawsPort.Dtos;
using PawsPort.Models;
using System.Diagnostics;
using System.Drawing;
using Microsoft.EntityFrameworkCore;
using Serilog;

namespace PawsPort.Services
{
    public class MemberProfileService
    {
        private readonly PetDbContext _context;

        public MemberProfileService(PetDbContext context)
        {
            _context = context;
        }

        public async Task<List<MemberUserDTO>> GetAllUserInfoAsync()
        {
            var users = await _context.UserTables
                .Where(x => x.DeleteDay == null)
                .Select(u => new MemberUserDTO
                {
                    UserId = u.UserId,
                    Name = u.Name,
                    Photo = u.Photo,
                    Job = u.Job,
                    Phone = u.Phone,
                    Birthday = u.Birthday,
                    City = u.City,
                    Point = u.Point,
                    Note = u.Note,
                    HasPriorExp = u.HasPriorExp,
                    Status = u.Status,
                    IsSubscribe = u.IsSubscribe,
                    IsVerify = u.IsVerify,
                    CreatedAt = u.CreatedAt,
                    UpdatedAt = u.UpdatedAt
                })
                .ToListAsync(); // 3. 這裡直接使用非同步擴充方法

            return users;
        }

        public async Task<MemberSummaryDTO> GetMemberSummaryAsync()
        {
            int memberCount = await _context.UserTables.CountAsync(x => x.DeleteDay == null);

            DateTime today = DateTime.Now;
            DateTime startOfMonth = new DateTime(today.Year, today.Month, 1);
            DateTime startOfNextMonth = startOfMonth.AddMonths(1);

            int memberMonthSignUp = await _context.UserTables.CountAsync(u => u.CreatedAt >= startOfMonth && u.CreatedAt < startOfNextMonth);
            int memberVerify = await _context.UserTables.CountAsync(x => x.IsVerify == true && x.DeleteDay == null);
            float verifyPercentage = memberCount > 0 ? ((float)memberVerify / memberCount) * 100 : 0;
            string displayVerify = verifyPercentage.ToString("F1");

            //Debug.WriteLine($"驗證比例: {displayVerify}%");
            int memberRss = await _context.UserTables.CountAsync(x => x.IsSubscribe == true && x.DeleteDay == null);
            return new MemberSummaryDTO
            {
                MemberCount = memberCount,
                MemberMonthSignUp = memberMonthSignUp,
                VerifyPercentage = displayVerify,
                SubscribedMemberCount = memberRss
            };
        }



        public async Task<CreateMemberDTO> CreateUserAsync(CreateMemberDTO userDto)
        {
            // 將 DTO 轉換為 EF Core 實體
            var userEntity = new UserTable
            {
                Name = userDto.Name,
                Photo = userDto.Photo,
                Job = userDto.Job,
                Phone = userDto.Phone,
                Birthday = userDto.Birthday,
                City = userDto.City,
                Point = userDto.Point ?? 0,
                Note = userDto.Note,
                HasPriorExp = userDto.HasPriorExp,
                Status = userDto.Status,
                IsSubscribe = userDto.IsSubscribe,
                IsVerify = userDto.IsVerify,
                CreatedAt = DateTime.Now,
                UpdatedAt = DateTime.Now,
                DeleteDay = null
            };

            _context.UserTables.Add(userEntity);
            _context.SaveChanges();

            // 將儲存後的實體（包含自動生成的 UserId）轉回 DTO
            userDto.CreatedAt = await Task.FromResult(userEntity.CreatedAt);
            userDto.UpdatedAt = await Task.FromResult(userEntity.UpdatedAt);

            return userDto;
        }


        public async Task<MemberUserDTO> GetUserInfoByIdAsync(int? id)
        {
            var user = await _context.UserTables
                .Where(x => x.UserId == id && x.DeleteDay == null)
                .Select(u => new MemberUserDTO
                {
                    UserId = u.UserId,
                    Name = u.Name,
                    Photo = u.Photo,
                    Job = u.Job,
                    Phone = u.Phone,
                    Birthday = u.Birthday,
                    City = u.City,
                    Point = u.Point,
                    Note = u.Note,
                    HasPriorExp = u.HasPriorExp,
                    Status = u.Status,
                    IsSubscribe = u.IsSubscribe,
                    IsVerify = u.IsVerify,
                    CreatedAt = u.CreatedAt,
                    UpdatedAt = u.UpdatedAt
                })
                .FirstOrDefaultAsync();

            return user;
        }


        public async Task<MemberUserDTO> UpdateUserInfoAsync(int id ,MemberUserDTO userDto)
        {
            var userEntity = await _context.UserTables
                .Where(m => m.UserId == id && m.DeleteDay == null)
                .FirstOrDefaultAsync();

            userEntity.Name = await Task.FromResult(userDto.Name);
            userEntity.Photo = await Task.FromResult(userDto.Photo);
            userEntity.Job = await Task.FromResult(userDto.Job);
            userEntity.Phone = await Task.FromResult(userDto.Phone);
            userEntity.Birthday = await Task.FromResult(userDto.Birthday);
            userEntity.City = await Task.FromResult(userDto.City);
            userEntity.Point = userDto.Point ?? 0;
            userEntity.Note = await Task.FromResult(userDto.Note);
            userEntity.HasPriorExp = await Task.FromResult(userDto.HasPriorExp);
            userEntity.Status = await Task.FromResult(userDto.Status);
            userEntity.IsSubscribe = await Task.FromResult(userDto.IsSubscribe);
            userEntity.IsVerify = await Task.FromResult(userDto.IsVerify);
            userEntity.UpdatedAt = DateTime.Now;

            await _context.SaveChangesAsync();

            userDto.UpdatedAt = await Task.FromResult(userEntity.UpdatedAt);

            return userDto;
        }


        public async Task<bool> DeleteUserAsync(int? id)
        {
            if (id == null)
                return false;

            var userEntity = await _context.UserTables
                .Where(m => m.UserId == id && m.DeleteDay == null)
                .FirstOrDefaultAsync();

            userEntity.DeleteDay = DateTime.Now;
            userEntity.UpdatedAt = DateTime.Now;

            await _context.SaveChangesAsync();

            return true;
        }

        public async Task<bool> CheckUserInfoAsync(int? id ,string? Email)
        {

            if (id.HasValue)
            {
                var userEntity = await _context.UserTables
               .Where(m => m.UserId == id && m.DeleteDay == null)
               .FirstOrDefaultAsync();

                if (userEntity == null || id == null)
                {
                    return false;
                }

                return true;
            }
            // 以後擴充方法

            return true;
        }

    }
}
