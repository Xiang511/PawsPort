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
                                      RoleId = r.RoleId,
                                      RoleName = r.RoleName,
                                      UpdatedAt = usr.UpdatedAt,
                                      MappingId = usr.MappingId
                                  };
            return View(userPermissions);
        }

        public IActionResult Create()
        {
            PetDbContext db = new PetDbContext();

            var viewModel = new PermissionCreateViewModel
            {
                Users = db.UserTables.Where(u => u.Status == true).ToList(),
                Systems = db.SystemTables.ToList(),
                Roles = db.RoleTables.ToList()
            };

            return View(viewModel);
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
            var userSystemRole = db.UserSystemRoles.Where(m => m.MappingId == id).FirstOrDefault();

            if (userSystemRole == null)
                return RedirectToAction("List");

            // 取得使用者名稱
            var user = db.UserTables.Where(u => u.UserId == userSystemRole.UserId).FirstOrDefault();

            // 建立 ViewModel
            var viewModel = new PermissionEditViewModel
            {
                MappingId = userSystemRole.MappingId,
                UserId = userSystemRole.UserId,
                UserName = user?.Name ?? "未知使用者",
                SystemId = userSystemRole.SystemId,
                RoleId = userSystemRole.RoleId,
                UpdatedAt = userSystemRole.UpdatedAt,
                Systems = db.SystemTables.ToList(),
                Roles = db.RoleTables.ToList()
            };

            return View(viewModel);
        }

        [HttpPost]
        public IActionResult Edit(PermissionEditViewModel viewModel)
        {
            PetDbContext db = new PetDbContext();
            UserSystemRole dbUserSystemRole = db.UserSystemRoles.Where(m => m.MappingId == viewModel.MappingId).FirstOrDefault();

            if (dbUserSystemRole != null)
            {
                dbUserSystemRole.UserId = viewModel.UserId;
                dbUserSystemRole.SystemId = viewModel.SystemId;
                dbUserSystemRole.RoleId = viewModel.RoleId;
                dbUserSystemRole.UpdatedAt = DateTime.Now;

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
            var bannedUsers = db.UserTables
                                .Where(u => u.Status == false)
                                .ToList();
            return View(bannedUsers);
        }


        public IActionResult BlockListCreate()
        {   
            PetDbContext db = new PetDbContext();
            List<BlockListViewModel> userList = db.UserTables.Where(m=>m.Status == true).Select(u => new BlockListViewModel()
            {
                UserId = u.UserId,
                Name = u.Name,
                Status = u.Status,
                Note = u.Note,
                UpdatedAt = u.UpdatedAt
            }).ToList();


            return View(userList);
        }

        [HttpPost]
        public IActionResult BlockListCreate(BlockListViewModel vm)
        {
            PetDbContext db = new PetDbContext();
            UserTable x = db.UserTables.Where(m => m.UserId == vm.UserId).FirstOrDefault();
            if(vm.Status == false && vm.Note!=null)
            {
                if (x != null )
                {
                    x.Status = vm.Status;
                    x.UpdatedAt = DateTime.Now;
                    x.Note = vm.Note;
                    db.SaveChanges();
                }
                db.SaveChanges();
                
            }
            return RedirectToAction("BlockList");
        }



        public IActionResult BlockListEdit(int? id)
        {
            if (id == null)
                return RedirectToAction("BlockList");

            PetDbContext db = new PetDbContext();
            var user = db.UserTables.Where(m => m.UserId == id).Select(u => new BlockListViewModel()
            {
                UserId = u.UserId,
                Name = u.Name,
                Status = u.Status,
                Note = u.Note,
                UpdatedAt = u.UpdatedAt
            }).FirstOrDefault();

            if (user == null)
                return RedirectToAction("BlockList");

            return View(user);
        }

        [HttpPost]
        public IActionResult BlockListEdit(BlockListViewModel vm)
        {
            PetDbContext db = new PetDbContext();
            UserTable dbUserTable = db.UserTables.Where(m => m.UserId == vm.UserId).FirstOrDefault();

            if (dbUserTable != null)
            {
                dbUserTable.Name = vm.Name;
                dbUserTable.Status = vm.Status;
                dbUserTable.UpdatedAt = DateTime.Now;
                dbUserTable.Note = vm.Note;
                db.SaveChanges();
            }

            return RedirectToAction("BlockList");
        }

        public IActionResult BlockListDelete(int? id)
        {
            if (id == null)
                return RedirectToAction("BlockList");

            PetDbContext db = new PetDbContext();
            UserTable dbUserTable = db.UserTables.Where(m => m.UserId == id).FirstOrDefault();
            if (dbUserTable != null)
            {
                dbUserTable.Status = true;
                dbUserTable.UpdatedAt = DateTime.Now;
                db.SaveChanges();
            }
           
            return RedirectToAction("BlockList");
        }
    }
}
