using Humanizer;
using Microsoft.EntityFrameworkCore;
using PawsPort.Dtos;
using PawsPort.Models;
using PawsPort.ViewModels;
using Serilog;
using static Microsoft.CodeAnalysis.CSharp.SyntaxTokenParser;

namespace PawsPort.Services
{
    public class MemberPermissionService
    {
        private readonly PetDbContext _context;

        public MemberPermissionService(PetDbContext context)
        {
            _context = context;
        }
        public async Task<List<MemberPermissionUserDTO>> GetAllUserPermissionAsync()
        {
            var userPermissions = await _context.UserTables
                .Where(u => u.DeleteDay == null)
                .Join(_context.UserSystemRoles,
                    u => u.UserId,
                    usr => usr.UserId,
                    (u, usr) => new { u, usr })
                .Join(_context.SystemTables,
                    temp => temp.usr.SystemId,
                    s => s.SystemId,
                    (temp, s) => new { temp.u, temp.usr, s })
                .Join(_context.RoleTables,
                    temp => temp.usr.RoleId,
                    r => r.RoleId,
                    (temp, r) => new MemberPermissionUserDTO
                    {
                        UserId = temp.u.UserId,
                        UserName = temp.u.Name,
                        SystemName = temp.s.SystemName,
                        RoleId = r.RoleId,
                        RoleName = r.RoleName,
                        UpdatedAt = temp.usr.UpdatedAt,
                        MappingId = temp.usr.MappingId,
                        SystemId = temp.s.SystemId
                    }).ToListAsync();

            return userPermissions;
        }

        public async Task<MemberPermissionSystemDTO> GetMemberPermissionSystemAsync()
        {
            var result = new MemberPermissionSystemDTO
            {
                Systems = await _context.SystemTables.ToListAsync()
            };
            return result;
        }
        public async Task<MemberPermissionRoleDTO> GetMemberPermissionRoleAsync()
        {
            var result = new MemberPermissionRoleDTO
            {
                Roles = await _context.RoleTables.ToListAsync()
            };
            return result;
        }

        public async Task<bool> CreateMemberPermissionAsync(MemberUserSystemRoleDTO user)
        {


            var userEntity = new UserSystemRole
            {
                UserId = user.UserId,
                SystemId = user.SystemId,
                RoleId = user.RoleId,
                UpdatedAt = DateTime.UtcNow
            };
            _context.UserSystemRoles.Add(userEntity);
            _context.SaveChanges();

            return true;

        }
        public async Task<bool> UpdateMemberPermissionRoleAsync(int? mappingId, MemberPermissionUpdateRoleDTO user)
        {

            var userEntity = await _context.UserSystemRoles.FindAsync(mappingId);
            if (userEntity == null || userEntity.UserId != user.UserId)
            {
                return false;
            }
            userEntity.RoleId = user.RoleId;
            userEntity.SystemId = user.SystemId;
            userEntity.UpdatedAt = DateTime.UtcNow;
            _context.UserSystemRoles.Update(userEntity);
            _context.SaveChanges();

            return true;

        }
        public async Task<bool> DeleteMemberPermissionRoleAsync(int? mappingId)
        {
            var userEntity = await _context.UserSystemRoles.FindAsync(mappingId);
            if (userEntity == null)
            {
                return false;
            }
            _context.UserSystemRoles.Remove(userEntity);
            _context.SaveChanges();

            return true;

        }
        public async Task<bool> CheckMemberPermissionExistAsync(MemberPermissionUpdateRoleDTO user)
        {
            var exists = await _context.UserSystemRoles.AnyAsync(usr => usr.UserId == user.UserId
               && usr.SystemId == user.SystemId
               && usr.RoleId == user.RoleId);

            return exists;

        }
        public async Task<bool> CheckMemberPermissionExistAsync(MemberUserSystemRoleDTO user)
        {
            var exists = await _context.UserSystemRoles.AnyAsync(usr => usr.UserId == user.UserId
               && usr.SystemId == user.SystemId
               && usr.RoleId == user.RoleId);

            return exists;


        }

        public async Task<List<MemberPermissionUserDTO>> GetUserPermissionsAsync(int? userId)
        {
            // 方法入口 (Entry)
            Log.Information("[PermissionService] GetUserPermissions - Entry. Target UserId: {UserId}", userId);

            var result = await _context.UserTables
                .Where(u => u.UserId == userId && u.DeleteDay == null)
                .Join(_context.UserSystemRoles,
                    u => u.UserId,
                    usr => usr.UserId,
                    (u, usr) => new { u, usr })
                .Join(_context.SystemTables,
                    temp => temp.usr.SystemId,
                    s => s.SystemId,
                    (temp, s) => new { temp.u, temp.usr, s })
                .Join(_context.RoleTables,
                    temp => temp.usr.RoleId,
                    r => r.RoleId,
                    (temp, r) => new MemberPermissionUserDTO
                    {
                        UserId = temp.u.UserId,
                        UserName = temp.u.Name,
                        SystemName = temp.s.SystemName,
                        RoleId = r.RoleId,
                        RoleName = r.RoleName,
                        UpdatedAt = temp.usr.UpdatedAt,
                        MappingId = temp.usr.MappingId,
                        SystemId = temp.s.SystemId
                    }).ToListAsync();

            // 方法出口 (Exit)
            // 注意：ToListAsync() 沒找到資料會回傳「空清單 []」而不是 null
            if (result == null || result.Count == 0)
            {
                Log.Warning("[PermissionService] GetUserPermissions - Exit. No permissions found. UserId: {UserId}", userId);
                return new List<MemberPermissionUserDTO>(); // 建議回傳空清單，避免前端發生 NullReference
            }

            // 修正點：Log 改為記錄「取得幾筆」資料，而不是單一 MappingId
            Log.Information(
                "[PermissionService] GetUserPermissions - Exit. Success. Found {Count} permissions for UserId: {UserId}",
                result.Count, userId);

            return result;
        }
    }
}
