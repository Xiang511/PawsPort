using Microsoft.AspNetCore.Mvc;
using PawsPort.Models;
using PawsPort.ViewModels;

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
                                      AssignedAt = usr.AssignedAt,
                                      MappingId= usr.MappingId
                                  };
            return View(userPermissions);
        }

        public IActionResult Create()
        {   
            return View();
        }

        [HttpPost]
        public IActionResult Create(UserSystemRole US)
        {
            PetDbContext db = new PetDbContext();
            db.UserSystemRoles.Add(US);
            db.SaveChanges();
            return RedirectToAction("List");
        }


        public IActionResult Edit(int? id)
        {
            if (id == null)
                return RedirectToAction("List");

            PetDbContext db = new PetDbContext();
            UserSystemRole x = db.UserSystemRoles.Where(m => m.UserId == id).FirstOrDefault();

            if (x == null)
                return RedirectToAction("List");

            return View(x);
        }

        [HttpPost]
        public IActionResult Edit(UserSystemRole uiUserSystemRole)
        {
            PetDbContext db = new PetDbContext();
            UserSystemRole dbUserSystemRole = db.UserSystemRoles.Where(m => m.UserId == uiUserSystemRole.UserId).FirstOrDefault();

            if (dbUserSystemRole != null)
            {
                dbUserSystemRole.SystemId = uiUserSystemRole.SystemId;
                dbUserSystemRole.RoleId = uiUserSystemRole.RoleId;
                dbUserSystemRole.AssignedAt = uiUserSystemRole.AssignedAt;
                dbUserSystemRole.UserId = uiUserSystemRole.UserId;

                db.SaveChanges();
            }

            return RedirectToAction("List");
        }

        public IActionResult Delete(int? id)
        {
            if (id == null)
                return RedirectToAction("List");

            PetDbContext db = new PetDbContext();
            UserSystemRole x = db.UserSystemRoles.Where(m => m.MappingId == id).FirstOrDefault();

            if (x != null)
            {
                db.UserSystemRoles.Remove(x);
                db.SaveChanges();
            }

            return RedirectToAction("List");
        }



        public IActionResult BlockList()
        {
            PetDbContext db = new PetDbContext();
            var userPermissions = from u in db.UserTables
                                  join usr in db.UserSystemRoles on u.UserId equals usr.UserId
                                  join s in db.SystemTables on usr.SystemId equals s.SystemId
                                  join r in db.RoleTables on usr.RoleId equals r.RoleId
                                  where r.RoleId == 3
                                  // 可加入過濾條件，例如查詢特定使用者 ID
                                  // where u.UserId == 1 
                                  select new UserPermissionViewModel // <--- 必須指定類別名稱
                                  {
                                      UserId = u.UserId,
                                      UserName = u.Name,
                                      SystemName = s.SystemName,
                                      RoleName = r.RoleName,
                                      AssignedAt = usr.AssignedAt,
                                      MappingId = usr.MappingId
                                  };
            return View(userPermissions);
        }


        public IActionResult BlockListCreate()
        {
            return View();
        }

        [HttpPost]
        public IActionResult BlockListCreate(UserSystemRole US)
        {
            PetDbContext db = new PetDbContext();
            db.UserSystemRoles.Add(US);
            db.SaveChanges();
            return RedirectToAction("BlockList");
        }

        public IActionResult BlockListEdit(int? id)
        {
            if (id == null)
                return RedirectToAction("BlockList");

            PetDbContext db = new PetDbContext();
            UserSystemRole x = db.UserSystemRoles.Where(m => m.UserId == id).FirstOrDefault();

            if (x == null)
                return RedirectToAction("BlockList");

            return View(x);
        }

        [HttpPost]
        public IActionResult BlockListEdit(UserSystemRole uiUserSystemRole)
        {
            PetDbContext db = new PetDbContext();
            UserSystemRole dbUserSystemRole = db.UserSystemRoles.Where(m => m.UserId == uiUserSystemRole.UserId).FirstOrDefault();

            if (dbUserSystemRole != null)
            {
                dbUserSystemRole.SystemId = uiUserSystemRole.SystemId;
                dbUserSystemRole.RoleId = uiUserSystemRole.RoleId;
                dbUserSystemRole.AssignedAt = uiUserSystemRole.AssignedAt;
                dbUserSystemRole.UserId = uiUserSystemRole.UserId;

                db.SaveChanges();
            }

            return RedirectToAction("BlockList");
        }

        public IActionResult BlockListDelete(int? id)
        {
            if (id == null)
                return RedirectToAction("BlockList");

            PetDbContext db = new PetDbContext();
            UserSystemRole x = db.UserSystemRoles.Where(m => m.MappingId == id).FirstOrDefault();

            if (x != null)
            {
                db.UserSystemRoles.Remove(x);
                db.SaveChanges();
            }

            return RedirectToAction("BlockList");
        }
    }
}
