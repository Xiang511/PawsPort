using Microsoft.AspNetCore.Mvc;
using PawsPort.Dtos;
using PawsPort.Models;
using PawsPort.Services;
using PawsPort.ViewModels;
using Serilog;

namespace PawsPort.Controllers
{

    [ApiController]
    [Route("api/[controller]")]
    [Produces("application/json")]
    public class PermissionsController : ApiControllerBase
    {
        private readonly PetDbContext _context;
        private readonly MemberPermissionService _memberPermissionService;


        public PermissionsController(PetDbContext context, MemberPermissionService memberPermissionService)
        {
            _context = context;
            _memberPermissionService = memberPermissionService;
        }

        /// <summary>
        /// 取得所有使用者的權限列表
        /// </summary>
        /// <returns>使用者權限列表 JSON</returns>
        /// <response code="200">成功取得使用者權限列表</response>

        [HttpGet("users")]
        [ProducesResponseType(typeof(IEnumerable<MemberPermissionUserDTO>), StatusCodes.Status200OK)]

        public async Task<IActionResult> UserPermission()
        {

            var userPermissions = await _memberPermissionService.GetAllUserPermissionAsync();
            Log.Information("取得使用者權限列表成功，總數: {Count}", userPermissions.Count());
            return Success(userPermissions, "Success", 200);
        }
        /// <summary>
        /// 取得所有系統列表
        /// </summary>
        /// <returns>系統列表 JSON</returns>
        /// <response code="200">成功取得系統列表</response>
        [HttpGet("systems")]
        [ProducesResponseType(typeof(MemberPermissionSystemDTO), StatusCodes.Status200OK)]
 
        public async Task<IActionResult> System()
        {

            var result = await _memberPermissionService.GetMemberPermissionSystemAsync();

            Log.Information("取得系統列表成功，總數: {Count}", result.Systems.Count());
            return Success(result, "Success", 200);
        }

        /// <summary>
        /// 取得所有角色列表
        /// </summary>
        /// <returns>角色列表 JSON</returns>
        /// <response code="200">成功取得角色列表</response>

        [HttpGet("roles")]
        [ProducesResponseType(typeof(MemberPermissionRoleDTO), StatusCodes.Status200OK)]

        public async Task<IActionResult> Roles()
        {

            var result = await _memberPermissionService.GetMemberPermissionRoleAsync();


            Log.Information("取得角色列表成功，總數: {Count}", result.Roles.Count());
            return Success(result, "Success", 200);
        }

        /// <summary>
        /// 新增使用者權限
        /// </summary>
        /// <param name="US">使用者權限資料（包含 UserId、SystemId、RoleId）</param>
        /// <returns>操作結果 JSON</returns>
        /// <response code="200">成功新增使用者權限</response>
        /// <response code="204">新增失敗（可能因為權限已存在或參數無效）</response>
        [HttpPost("users")]
        [ProducesResponseType(typeof(bool), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        public async Task<IActionResult> Create(MemberUserSystemRoleDTO US)
        {
            bool exist = await _memberPermissionService.CheckMemberPermissionExistAsync(US);

            if (exist)
            {   
                Log.Warning("嘗試新增已存在的使用者權限: UserId={UserId}, SystemId={SystemId}, RoleId={RoleId}", US.UserId, US.SystemId, US.RoleId);
                return Failure("PERMISSION_ALREADY_EXISTS", "該使用者已擁有此系統的相同角色權限", 409);
            }

            var result = await _memberPermissionService.CreateMemberPermissionAsync(US);

            if (result == false)
            {   
                Log.Warning("新增使用者權限失敗: UserId={UserId}, SystemId={SystemId}, RoleId={RoleId}", US.UserId, US.SystemId, US.RoleId);
                return NoContent();
            }

            Log.Information("成功新增使用者權限: UserId={UserId}, SystemId={SystemId}, RoleId={RoleId}", US.UserId, US.SystemId, US.RoleId);
            return Success(result, "Success", 200);
        }

        /// <summary>
        /// 更新使用者權限
        /// </summary>
        /// <param name="mappingId">權限對應 ID</param>
        /// <param name="user">更新的權限資料（包含 UserId、SystemId、RoleId）</param>
        /// <returns>操作結果</returns>
        /// <response code="204">成功更新使用者權限</response>
        /// <response code="400">無效的權限對應 ID（ID 為空或小於 0）</response>
        /// <response code="409">權限衝突或權限不存在</response>
        [HttpPatch("roles/{mappingId}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]

        public async Task<IActionResult> Edit(int? mappingId, MemberPermissionUpdateRoleDTO user)
        {
            // 驗證路由參數：mappingId 不可為 null 或小於 0
            if (mappingId == null || mappingId < 0)
            {
                Log.Warning("無效的權限對應 ID: {MappingId}", mappingId);
                return Failure("INVALID_MAPPING_ID", "無效ID", 400);
            }
                


            bool exist = await _memberPermissionService.CheckMemberPermissionExistAsync(user);

            if (exist)
            {   
                Log.Warning("嘗試更新為已存在的使用者權限: UserId={UserId}, SystemId={SystemId}, RoleId={RoleId}", user.UserId, user.SystemId, user.RoleId);
                return Failure("PERMISSION_ALREADY_EXISTS", "該使用者已擁有相同角色權限", 409);
            }

            bool result = await _memberPermissionService.UpdateMemberPermissionRoleAsync(mappingId, user);


            if (!result)
            {
                Log.Warning("找不到指定的使用者權限: MappingId={MappingId}", mappingId);
                return Failure("User_NOT_FOUND", "找不到指定的Id或是使用者", 404);
            }

            Log.Information("成功更新使用者權限: MappingId={MappingId}, UserId={UserId}, SystemId={SystemId}, RoleId={RoleId}", mappingId, user.UserId, user.SystemId, user.RoleId);
            return NoContent();

        }

        /// <summary>
        /// 刪除使用者權限
        /// </summary>
        /// <param name="mappingId">權限對應 ID</param>
        /// <returns>操作結果</returns>
        /// <response code="204">成功刪除使用者權限</response>
        /// <response code="400">無效的權限對應 ID（ID 為空）</response>
        /// <response code="404">找不到指定的權限</response>

        [HttpDelete("roles/{mappingId}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> Delete(int? mappingId)
        {
            // 驗證路由參數：mappingId 不可為 null
            if (mappingId == null)
            {
                Log.Information("刪除使用者權限失敗，無效的權限對應 ID: {MappingId}", mappingId);
                return Failure("INVALID_MAPPING_ID", "無效ID", 400);
            }


            bool result = await _memberPermissionService.DeleteMemberPermissionRoleAsync(mappingId);

            if (!result)
            {   
                Log.Warning("找不到指定的使用者權限，刪除失敗: MappingId={MappingId}", mappingId);
                return Failure("PERMISSION_NOT_FOUND", "找不到指定的權限", 404);
            }

            Log.Information("成功刪除使用者權限: MappingId={MappingId}", mappingId);
            return NoContent();
        }




        //public IActionResult BlockList()
        //{
        //    var bannedUsers = _context.UserTables
        //                        .Where(u => u.Status == false && u.DeleteDay == null)
        //                        .ToList();
        //    return View(bannedUsers);
        //}


        //public IActionResult BlockListCreate()
        //{   
        //    List<BlockListViewModel> userList = _context.UserTables.Where(m=>m.Status == true).Select(u => new BlockListViewModel()
        //    {
        //        UserId = u.UserId,
        //        Name = u.Name,
        //        Status = u.Status,
        //        Note = u.Note,
        //        UpdatedAt = u.UpdatedAt
        //    }).ToList();


        //    return View(userList);
        //}

        //[HttpPost]
        //public IActionResult BlockListCreate(BlockListViewModel vm)
        //{
        //    UserTable x = _context.UserTables.Where(m => m.UserId == vm.UserId).FirstOrDefault();
        //    if(vm.Status == false && vm.Note!=null)
        //    {
        //        if (x != null )
        //        {
        //            x.Status = vm.Status;
        //            x.UpdatedAt = DateTime.Now;
        //            x.Note = vm.Note;
        //            _context.SaveChanges();
        //        }

        //    }
        //    return RedirectToAction("BlockList");
        //}



        //public IActionResult BlockListEdit(int? id)
        //{
        //    if (id == null)
        //        return RedirectToAction("BlockList");

        //    var user = _context.UserTables.Where(m => m.UserId == id).Select(u => new BlockListViewModel()
        //    {
        //        UserId = u.UserId,
        //        Name = u.Name,
        //        Status = u.Status,
        //        Note = u.Note,
        //        UpdatedAt = u.UpdatedAt
        //    }).FirstOrDefault();

        //    if (user == null)
        //        return RedirectToAction("BlockList");

        //    return View(user);
        //}

        //[HttpPost]
        //public IActionResult BlockListEdit(BlockListViewModel vm)
        //{
        //    UserTable dbUserTable = _context.UserTables.Where(m => m.UserId == vm.UserId).FirstOrDefault();

        //    if (dbUserTable != null)
        //    {
        //        dbUserTable.Name = vm.Name;
        //        dbUserTable.Status = vm.Status;
        //        dbUserTable.UpdatedAt = DateTime.Now;
        //        dbUserTable.Note = vm.Note;
        //        _context.SaveChanges();
        //    }

        //    return RedirectToAction("BlockList");
        //}

        //public IActionResult BlockListDelete(int? id)
        //{
        //    if (id == null)
        //        return RedirectToAction("BlockList");

        //    UserTable dbUserTable = _context.UserTables.Where(m => m.UserId == id).FirstOrDefault();
        //    if (dbUserTable != null)
        //    {
        //        dbUserTable.Status = true;
        //        dbUserTable.UpdatedAt = DateTime.Now;
        //        _context.SaveChanges();
        //    }

        //    return RedirectToAction("BlockList");
        //}
    }
}
