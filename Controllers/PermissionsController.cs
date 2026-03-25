using Microsoft.AspNetCore.Mvc;
using PawsPort.Models;
using PawsPort.Models.ViewModels;

namespace PawsPort.Controllers
{
    public class PermissionsController : Controller
    {
        public IActionResult List()
        {
            PetDbContext db = new PetDbContext();
            var userPermissions = from u in db.UserTables
                                  join usr in db.UserSystemRoles on u.UserId equals usr.UserId
                                  join s in db.SystemTables on usr.SystemId equals s.SystemId
                                  join r in db.RoleTables on usr.RoleId equals r.RoleId
                                  // 可加入過濾條件，例如查詢特定使用者 ID
                                  // where u.UserId == 1 
                                  select new UserPermissionViewModel // <--- 必須指定類別名稱
                                  {
                                      UserId = u.UserId,
                                      UserName = u.Name,
                                      SystemName = s.SystemName,
                                      RoleName = r.RoleName,
                                      AssignedAt = usr.AssignedAt
                                  };
            return View(userPermissions);
        }
    }
}
