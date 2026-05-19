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
            Log.Debug("[MemberProfileService] GetAllUserInfoAsync - Entry");

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

            Log.Debug("[MemberProfileService] GetAllUserInfoAsync - Exit, 返回用戶數量: {Count}", users.Count);
            return users;
        }

        public async Task<MemberSummaryDTO> GetMemberSummaryAsync()
        {
            Log.Debug("[MemberProfileService] GetMemberSummaryAsync - Entry");

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

            Log.Debug("[MemberProfileService] GetMemberSummaryAsync - Exit, 會員總數: {MemberCount}, 本月新增: {MonthSignUp}, 驗證比例: {VerifyPercentage}%", memberCount, memberMonthSignUp, displayVerify);
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
            Log.Debug("[MemberProfileService] CreateUserAsync - Entry, 用戶名稱: {Name}", userDto.Name);

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

            Log.Debug("[MemberProfileService] CreateUserAsync - Exit, 成功創建用戶: {Name}", userDto.Name);
            return userDto;
        }


        public async Task<MemberUserDTO> GetUserInfoByIdAsync(int? id)
        {
            Log.Debug("[MemberProfileService] GetUserInfoByIdAsync - Entry, UserId: {UserId}", id);

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

            Log.Debug("[MemberProfileService] GetUserInfoByIdAsync - Exit, UserId: {UserId}, 找到用戶: {Found}", id, user != null);
            return user;
        }

        public async Task<MemberUserDTO> GetUserInfoByEmailAsync(string email)
        {
            Log.Debug("[MemberProfileService] GetUserInfoByEmailAsync - Entry, Email: {Email}", email);

            var user = await _context.UserAuthTables
                .Where(x => x.Email == email)
                .Join(
                    _context.UserTables.Where(u => u.DeleteDay == null),
                    auth => auth.UserId,
                    userTable => userTable.UserId,
                    (auth, userTable) => new MemberUserDTO
                    {
                        UserId = userTable.UserId,
                        Name = userTable.Name,
                        Photo = userTable.Photo,
                        Job = userTable.Job,
                        Phone = userTable.Phone,
                        Birthday = userTable.Birthday,
                        City = userTable.City,
                        Point = userTable.Point,
                        Note = userTable.Note,
                        HasPriorExp = userTable.HasPriorExp,
                        Status = userTable.Status,
                        IsSubscribe = userTable.IsSubscribe,
                        IsVerify = userTable.IsVerify,
                        CreatedAt = userTable.CreatedAt,
                        UpdatedAt = userTable.UpdatedAt
                    })
                .FirstOrDefaultAsync();

            Log.Debug("[MemberProfileService] GetUserInfoByEmailAsync - Exit, Email: {Email}, 找到用戶: {Found}", email, user != null);
            return user;
        }


        public async Task<MemberUserDTO> UpdateUserInfoAsync(int id ,MemberUserDTO userDto)
        {
            Log.Debug("[MemberProfileService] UpdateUserInfoAsync - Entry, UserId: {UserId}, 用戶名稱: {Name}", id, userDto.Name);

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

            Log.Debug("[MemberProfileService] UpdateUserInfoAsync - Exit, UserId: {UserId}, 更新成功", id);
            return userDto;
        }


        public async Task<bool> DeleteUserAsync(int? id)
        {
            Log.Debug("[MemberProfileService] DeleteUserAsync - Entry, UserId: {UserId}", id);

            if (id == null)
            {
                Log.Warning("[MemberProfileService] DeleteUserAsync - Exit, UserId 為 null, 返回 false");
                return false;
            }

            var userEntity = await _context.UserTables
                .Where(m => m.UserId == id && m.DeleteDay == null)
                .FirstOrDefaultAsync();

            if (userEntity == null)
            {
                Log.Warning("[MemberProfileService] DeleteUserAsync - Exit, 找不到 UserId: {UserId}, 返回 false", id);
                return false;
            }

            userEntity.DeleteDay = DateTime.Now;
            userEntity.UpdatedAt = DateTime.Now;

            await _context.SaveChangesAsync();

            Log.Debug("[MemberProfileService] DeleteUserAsync - Exit, UserId: {UserId}, 刪除成功", id);
            return true;
        }

        public async Task<bool> CheckUserInfoAsync(int? id ,string? Email)
        {
            Log.Debug("[MemberProfileService] CheckUserInfoAsync - Entry, UserId: {UserId}, Email: {Email}", id, Email);

            if (id.HasValue)
            {
                var userEntity = await _context.UserTables
               .Where(m => m.UserId == id && m.DeleteDay == null)
               .FirstOrDefaultAsync();

                if (userEntity == null || id == null)
                {
                    Log.Debug("[MemberProfileService] CheckUserInfoAsync - Exit, UserId: {UserId}, 用戶不存在, 返回 false", id);
                    return false;
                }

                Log.Debug("[MemberProfileService] CheckUserInfoAsync - Exit, UserId: {UserId}, 用戶存在, 返回 true", id);
                return true;
            }
            // 以後擴充方法

            Log.Debug("[MemberProfileService] CheckUserInfoAsync - Exit, 無有效檢查條件, 返回 true");
            return true;
        }

    }
}
