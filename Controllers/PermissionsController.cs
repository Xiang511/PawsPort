using Microsoft.AspNetCore.Mvc;
using PawsPort.Models;
using PawsPort.ViewModels;

namespace PawsPort.Controllers
{
    public class PermissionsController : Controller
    {
        private readonly PetDbContext _context;

        public PermissionsController(PetDbContext context)
        {
            _context = context;
        }

        public IActionResult List()
        {
            var userPermissions = from u in _context.UserTables
                                  join usr in _context.UserSystemRoles on u.UserId equals usr.UserId
                                  join s in _context.SystemTables on usr.SystemId equals s.SystemId
                                  join r in _context.RoleTables on usr.RoleId equals r.RoleId
                                  where u.DeleteDay == null 
                                  select new UserPermissionViewModel
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
            var viewModel = new PermissionCreateViewModel
            {
                Users = _context.UserTables.Where(u => u.Status == true).ToList(),
                Systems = _context.SystemTables.ToList(),
                Roles = _context.RoleTables.ToList()
            };

            return View(viewModel);
        }

        [HttpPost]
        public IActionResult Create(UserSystemRole US)
        {
            _context.UserSystemRoles.Add(US);
            _context.SaveChanges();
            return RedirectToAction("List");
        }


        public IActionResult Edit(int? id)
        {
            if (id == null)
                return RedirectToAction("List");

            var userSystemRole = _context.UserSystemRoles.Where(m => m.MappingId == id).FirstOrDefault();

            if (userSystemRole == null)
                return RedirectToAction("List");

            // 取得使用者名稱
            var user = _context.UserTables.Where(u => u.UserId == userSystemRole.UserId).FirstOrDefault();

            // 建立 ViewModel
            var viewModel = new PermissionEditViewModel
            {
                MappingId = userSystemRole.MappingId,
                UserId = userSystemRole.UserId,
                UserName = user?.Name ?? "未知使用者",
                SystemId = userSystemRole.SystemId,
                RoleId = userSystemRole.RoleId,
                UpdatedAt = userSystemRole.UpdatedAt,
                Systems = _context.SystemTables.ToList(),
                Roles = _context.RoleTables.ToList()
            };

            return View(viewModel);
        }

        [HttpPost]
        public IActionResult Edit(PermissionEditViewModel viewModel)
        {
            UserSystemRole dbUserSystemRole = _context.UserSystemRoles.Where(m => m.MappingId == viewModel.MappingId).FirstOrDefault();

            if (dbUserSystemRole != null)
            {
                dbUserSystemRole.UserId = viewModel.UserId;
                dbUserSystemRole.SystemId = viewModel.SystemId;
                dbUserSystemRole.RoleId = viewModel.RoleId;
                dbUserSystemRole.UpdatedAt = DateTime.Now;

                _context.SaveChanges();
            }

            return RedirectToAction("List");
        }

        public IActionResult Delete(int? id)
        {
            if (id == null)
                return RedirectToAction("List");

            UserSystemRole x = _context.UserSystemRoles.Where(m => m.MappingId == id).FirstOrDefault();

            if (x != null)
            {
                _context.UserSystemRoles.Remove(x);
                _context.SaveChanges();
            }

            return RedirectToAction("List");
        }



        public IActionResult BlockList()
        {
            var bannedUsers = _context.UserTables
                                .Where(u => u.Status == false && u.DeleteDay == null)
                                .ToList();
            return View(bannedUsers);
        }


        public IActionResult BlockListCreate()
        {   
            List<BlockListViewModel> userList = _context.UserTables.Where(m=>m.Status == true).Select(u => new BlockListViewModel()
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
            UserTable x = _context.UserTables.Where(m => m.UserId == vm.UserId).FirstOrDefault();
            if(vm.Status == false && vm.Note!=null)
            {
                if (x != null )
                {
                    x.Status = vm.Status;
                    x.UpdatedAt = DateTime.Now;
                    x.Note = vm.Note;
                    _context.SaveChanges();
                }

            }
            return RedirectToAction("BlockList");
        }



        public IActionResult BlockListEdit(int? id)
        {
            if (id == null)
                return RedirectToAction("BlockList");

            var user = _context.UserTables.Where(m => m.UserId == id).Select(u => new BlockListViewModel()
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
            UserTable dbUserTable = _context.UserTables.Where(m => m.UserId == vm.UserId).FirstOrDefault();

            if (dbUserTable != null)
            {
                dbUserTable.Name = vm.Name;
                dbUserTable.Status = vm.Status;
                dbUserTable.UpdatedAt = DateTime.Now;
                dbUserTable.Note = vm.Note;
                _context.SaveChanges();
            }

            return RedirectToAction("BlockList");
        }

        public IActionResult BlockListDelete(int? id)
        {
            if (id == null)
                return RedirectToAction("BlockList");

            UserTable dbUserTable = _context.UserTables.Where(m => m.UserId == id).FirstOrDefault();
            if (dbUserTable != null)
            {
                dbUserTable.Status = true;
                dbUserTable.UpdatedAt = DateTime.Now;
                _context.SaveChanges();
            }

            return RedirectToAction("BlockList");
        }
    }
}
