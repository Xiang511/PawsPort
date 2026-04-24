using Microsoft.AspNetCore.Http.HttpResults;
using PawsPort.Dtos;
using PawsPort.Models;
using System.Diagnostics;
using System.Drawing;

namespace PawsPort.Services
{
    public class MemberProfileService
    {
        private readonly PetDbContext _context;

        public MemberProfileService(PetDbContext context)
        {
            _context = context;
        }

        public List<MemberUserDTO> GetAllUserInfo()
        {
            var users = _context.UserTables
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
                .ToList();

            return users;
        }

        public MemberSummaryDTO GetMemberSummary()
        {
            int memberCount = _context.UserTables.Count(x => x.DeleteDay == null);

            DateTime today = DateTime.Now;
            DateTime startOfMonth = new DateTime(today.Year, today.Month, 1);
            DateTime startOfNextMonth = startOfMonth.AddMonths(1);

            int memberMonthSignUp = _context.UserTables.Count(u => u.CreatedAt >= startOfMonth && u.CreatedAt < startOfNextMonth);
            int memberVerify = _context.UserTables.Count(x => x.IsVerify == true && x.DeleteDay == null);
            float verifyPercentage = memberCount > 0 ? ((float)memberVerify / memberCount) * 100 : 0;
            string displayVerify = verifyPercentage.ToString("F1");

            //Debug.WriteLine($"驗證比例: {displayVerify}%");
            int memberRss = _context.UserTables.Count(x => x.IsSubscribe == true && x.DeleteDay == null);
            return new MemberSummaryDTO
            {
                MemberCount = memberCount,
                MemberMonthSignUp = memberMonthSignUp,
                VerifyPercentage = displayVerify,
                SubscribedMemberCount = memberRss
            };
        }



        public MemberUserDTO CreateUser(MemberUserDTO userDto)
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
            userDto.UserId = userEntity.UserId;
            userDto.CreatedAt = userEntity.CreatedAt;
            userDto.UpdatedAt = userEntity.UpdatedAt;

            return userDto;
        }


        public MemberUserDTO GetUserInfoById(int? id)
        {
            if (id == null)
                return null;

            var user = _context.UserTables
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
                .FirstOrDefault();

            return user;
        }


        public bool UpdateUserInfo(MemberUserDTO userDto)
        {
            var userEntity = _context.UserTables
                .Where(m => m.UserId == userDto.UserId && m.DeleteDay == null)
                .FirstOrDefault();

            if (userEntity == null)
                return false;

            // 更新實體屬性
            userEntity.Name = userDto.Name;
            userEntity.Photo = userDto.Photo;
            userEntity.Job = userDto.Job;
            userEntity.Phone = userDto.Phone;
            userEntity.Birthday = userDto.Birthday;
            userEntity.City = userDto.City;
            userEntity.Point = userDto.Point ?? 0;
            userEntity.Note = userDto.Note;
            userEntity.HasPriorExp = userDto.HasPriorExp;
            userEntity.Status = userDto.Status;
            userEntity.IsSubscribe = userDto.IsSubscribe;
            userEntity.IsVerify = userDto.IsVerify;
            userEntity.UpdatedAt = DateTime.Now;

            _context.SaveChanges();

            // 更新 DTO 的 UpdatedAt
            userDto.UpdatedAt = userEntity.UpdatedAt;

            return true;
        }


        public bool DeleteUser(int? id)
        {
            if (id == null)
                return false;

            var userEntity = _context.UserTables
                .Where(m => m.UserId == id && m.DeleteDay == null)
                .FirstOrDefault();

            if (userEntity == null)
                return false;

            userEntity.DeleteDay = DateTime.Now;
            userEntity.UpdatedAt = DateTime.Now;
            _context.SaveChanges();

            return true;
        }

    }
}
