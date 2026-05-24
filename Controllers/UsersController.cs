using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PawsPort.Dtos;
using PawsPort.Models;
using PawsPort.Services;
using Serilog;


using static Microsoft.CodeAnalysis.CSharp.SyntaxTokenParser;

namespace PawsPort.Controllers
{
    [Authorize(Policy = "會員系統_普通管理員")]
    [ApiController]
    [Route("api/[controller]")]
    [Produces("application/json")]

    public class UsersController : ApiControllerBase
    {
        private readonly MemberProfileService _memberProfileService;
        private readonly MemberPermissionService _memberPermissionService;
        private readonly PetAdoptionService _petAdoptionService;
        private readonly PetPassportService _petPassportService;
        private readonly PlayerService _playerService;
        private readonly ArticleService _articleService;
        private readonly FileService _fileService;
        private readonly IWebHostEnvironment _Env;

        public UsersController(PetDbContext context, MemberProfileService memberProfileService, MemberPermissionService memberPermissionService, PetAdoptionService petAdoptionService, PetPassportService petPassportService, PlayerService playerService, ArticleService articleService, FileService fileService, IWebHostEnvironment env)
        {
            _memberProfileService = memberProfileService;
            _memberPermissionService = memberPermissionService;
            _petAdoptionService = petAdoptionService;
            _petPassportService = petPassportService;
            _playerService = playerService;
            _articleService = articleService;
            _fileService = fileService;
            _Env = env;
        }

        /// <summary>
        /// 取得所有會員列表
        /// </summary>
        /// <returns>會員列表 JSON</returns>
        /// <response code="200">成功取得會員列表</response>

        [HttpGet]
        [ProducesResponseType(typeof(List<MemberUserDTO>), StatusCodes.Status200OK)]
        [Tags("會員管理")]
        public async Task<IActionResult> Members()
        {
            Log.Debug("[UsersController] Members GET - Entry");

            Log.Debug("[UsersController] 調用 MemberProfileService.GetAllUserInfoAsync");
            var users = await _memberProfileService.GetAllUserInfoAsync();
            Log.Debug("[UsersController] 取得會員資料成功, 共 {Count} 筆", users.Count);
            return Success(users, "Success", 200);
        }

        /// <summary>
        /// 取得指定會員資訊
        /// </summary>
        /// <param name="id">會員 ID</param>
        /// <returns>會員詳細資料</returns>
        /// <response code="200">成功取得會員資料</response>
        /// <response code="404">找不到指定的會員</response>

        [HttpGet("{id}")]
        [ProducesResponseType(typeof(MemberUserDTO), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [Tags("會員管理")]
        public async Task<IActionResult> Members(int? id)
        {
            Log.Debug("[UsersController] Members GET by id - Entry, UserId: {UserId}", id);

            Log.Debug("[UsersController] 調用 MemberProfileService.CheckUserInfoAsync, UserId: {UserId}", id);
            bool exist = await _memberProfileService.CheckUserInfoAsync(id, null);

            if (!exist)
            {
                Log.Warning("[UsersController] 會員不存在, UserId: {UserId}", id);
                return Failure("USER_NOT_FOUND", "找不到使用者", 404);
            }

            Log.Debug("[UsersController] 調用 MemberProfileService.GetUserInfoByIdAsync, UserId: {UserId}", id);
            var users = await _memberProfileService.GetUserInfoByIdAsync(id);
            Log.Debug("[UsersController] 取得會員資料成功, UserId: {UserId}, 名稱: {Name}", users.UserId, users.Name);
            return Success(users, "Success", 200);
        }



        /// <summary>
        /// 創建新會員
        /// </summary>
        /// <param name="createDto">會員註冊資料（UserId 會由系統自動生成）</param>
        /// <returns>創建成功的會員資料（包含自動生成的 UserId）</returns>
        /// <response code="200">成功創建會員</response>
        /// <response code="400">請求資料格式錯誤或必填欄位缺失</response>

        [HttpPost]
        [ProducesResponseType(typeof(MemberUserDTO), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [Tags("會員管理")]
        public async Task<IActionResult> Members(CreateMemberDTO user)
        {
            Log.Debug("[UsersController] Members POST - Entry, 會員名稱: {Name}", user.Name);

            Log.Debug("[UsersController] 調用 MemberProfileService.CreateUserAsync, 會員名稱: {Name}", user.Name);
            var result = await _memberProfileService.CreateUserAsync(user);

            Log.Debug("[UsersController] 創建會員成功, 名稱: {Name}", result.Name);

            return Success(result, "創建會員成功", 200);
        }

        /// <summary>
        /// 取得會員統計資訊（儀表板用）
        /// </summary>
        /// <returns>會員統計數據，包含總數、月註冊數、認證比例、訂閱數等</returns>
        /// <response code="200">成功取得統計資訊</response>

        [HttpGet("Summary")]
        [ProducesResponseType(typeof(MemberSummaryDTO), StatusCodes.Status200OK)]
        [Tags("會員管理")]
        public async Task<IActionResult> Summary()
        {
            Log.Debug("[UsersController] Summary GET - Entry");

            // 取得會員統計資訊
            Log.Debug("[UsersController] 調用 MemberProfileService.GetMemberSummaryAsync");
            var summary = await _memberProfileService.GetMemberSummaryAsync();

            Log.Debug("[UsersController] 成功取得統計資訊, 會員總數: {MemberCount}, 月註冊數: {MonthSignUp}, 認證比例: {VerifyPercentage}, 訂閱電子報人數: {SubscribedCount}",
                summary.MemberCount,
                summary.MemberMonthSignUp,
                summary.VerifyPercentage,
                summary.SubscribedMemberCount);

            return Success(summary, "成功取得統計資訊", 200);
        }


        /// <summary>
        /// 更新指定會員資訊
        /// </summary>
        /// <param name="id">會員ID</param>
        /// <param name="userDto">更新的會員資料</param>
        /// <returns>更新後的會員資料</returns>
        /// <response code="200">成功更新會員</response>
        /// <response code="400">會員 ID 不一致或資料格式錯誤</response>
        /// <response code="404">找不到指定的會員</response>

        [HttpPut("{id}")]
        [ProducesResponseType(typeof(MemberUserDTO), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [Tags("會員管理")]
        public async Task<IActionResult> UpdateMembers(int? id, MemberUserDTO userDto)
        {
            Log.Debug("[UsersController] UpdateMembers PUT - Entry, UserId: {UserId}, 會員名稱: {Name}", id, userDto.Name);

            if (id != userDto.UserId)
            {
                Log.Warning("[UsersController] 會員ID不一致, 路徑ID: {PathId}, DTO ID: {DtoId}", id, userDto.UserId);
                return Failure("USER_ID_MISMATCH", "會員ID不一致", 400);
            }

            Log.Debug("[UsersController] 調用 MemberProfileService.CheckUserInfoAsync, UserId: {UserId}", id);
            bool exist = await _memberProfileService.CheckUserInfoAsync(id, null);

            if (!exist)
            {
                Log.Warning("[UsersController] 會員不存在, UserId: {UserId}", id);
                return Failure("USER_NOT_FOUND", "找不到使用者", 404);
            }

            Log.Debug("[UsersController] 調用 MemberProfileService.UpdateUserInfoAsync, UserId: {UserId}", id);
            var result = await _memberProfileService.UpdateUserInfoAsync(id.Value, userDto);

            Log.Debug("[UsersController] 更新會員成功, 會員ID: {UserId}, 名稱: {Name}", userDto.UserId, userDto.Name);
            return Success(result, "Success", 200);

        }

        /// <summary>
        /// 刪除指定會員（軟刪除）
        /// </summary>
        /// <param name="id">會員 ID</param>
        /// <returns>無內容（204）或錯誤訊息</returns>
        /// <response code="204">成功刪除會員（軟刪除）</response>
        /// <response code="404">找不到指定會員</response>
        /// 

        [HttpPatch("{id}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [Tags("會員管理")]
        public async Task<IActionResult> DeleteMember(int? id)
        {
            Log.Debug("[UsersController] DeleteMember DELETE - Entry, UserId: {UserId}", id);

            Log.Debug("[UsersController] 調用 MemberProfileService.DeleteUserAsync, UserId: {UserId}", id);
            var result = await _memberProfileService.DeleteUserAsync(id);

            if (result)
            {
                Log.Debug("[UsersController] 刪除會員成功, 會員ID: {UserId}", id);
                return NoContent();
            }
            else
            {
                Log.Warning("[UsersController] 刪除會員失敗, 會員ID: {UserId}", id);
                return Failure("USER_NOT_FOUND", "找不到使用者", 404);
            }
        }

        /// <summary>
        /// 取得指定會員的權限列表
        /// </summary>
        /// <param name="id">使用者 ID</param>
        /// <returns>使用者的權限角色列表</returns>
        /// <response code="200">成功取得使用者權限</response>
        /// <response code="404">找不到指定的使用者</response>
        [Authorize(Policy = "會員系統_系統管理員")]
        [HttpGet("/api/users/{id}/roles")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [Tags("會員管理")]
        public async Task<IActionResult> QueryRoleById(int? id)
        {
            Log.Debug("[UsersController] QueryRoleById GET - Entry, UserId: {UserId}", id);

            Log.Debug("[UsersController] 調用 MemberPermissionService.GetUserPermissionsAsync, UserId: {UserId}", id);
            var userPermission = await _memberPermissionService.GetUserPermissionsAsync(id);

            if (userPermission == null)
            {
                Log.Warning("[UsersController] 找不到指定的使用者權限, UserId: {UserId}", id);
                return Failure("PERMISSION_NOT_FOUND", "找不到指定的使用者", 404);
            }

            Log.Debug("[UsersController] 成功取得使用者權限, UserId: {UserId}", id);
            return Success(userPermission, "Success", 200);
        }



        /// <summary>
        /// 新增會員權限
        /// </summary>
        /// <param name="US">使用者權限資料（包含 UserId、SystemId、RoleId）</param>
        /// <returns>操作結果 JSON</returns>
        /// <response code="200">成功新增使用者權限</response>
        /// <response code="204">新增失敗（可能因為權限已存在或參數無效）</response>
        /// <response code="409">權限衝突（該使用者已擁有此系統的相同角色權限）</response>
        [Authorize(Policy = "會員系統_系統管理員")]
        [HttpPost("/api/users/roles")]
        [ProducesResponseType(typeof(bool), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        [Tags("會員管理")]
        public async Task<IActionResult> CreateRole(MemberUserSystemRoleDTO US)
        {
            Log.Debug("[UsersController] CreateRole POST - Entry, UserId: {UserId}, SystemId: {SystemId}, RoleId: {RoleId}", US.UserId, US.SystemId, US.RoleId);

            Log.Debug("[UsersController] 調用 MemberPermissionService.CheckMemberPermissionExistAsync");
            bool exist = await _memberPermissionService.CheckMemberPermissionExistAsync(US);

            if (exist)
            {
                Log.Warning("[UsersController] 嘗試新增已存在的使用者權限: UserId={UserId}, SystemId={SystemId}, RoleId={RoleId}", US.UserId, US.SystemId, US.RoleId);
                return Failure("PERMISSION_ALREADY_EXISTS", "該使用者已擁有此系統的相同角色權限", 409);
            }

            Log.Debug("[UsersController] 調用 MemberPermissionService.CreateMemberPermissionAsync");
            var result = await _memberPermissionService.CreateMemberPermissionAsync(US);

            if (result == false)
            {
                Log.Warning("[UsersController] 新增使用者權限失敗: UserId={UserId}, SystemId={SystemId}, RoleId={RoleId}", US.UserId, US.SystemId, US.RoleId);
                return NoContent();
            }

            Log.Debug("[UsersController] 成功新增使用者權限: UserId={UserId}, SystemId={SystemId}, RoleId={RoleId}", US.UserId, US.SystemId, US.RoleId);
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
        [Authorize(Policy = "會員系統_系統管理員")]

        [HttpPatch("/api/users/{mappingId}/roles")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        [Tags("會員管理")]

        public async Task<IActionResult> EditRole(int? mappingId, MemberPermissionUpdateRoleDTO user)
        {
            Log.Debug("[UsersController] EditRole PATCH - Entry, MappingId: {MappingId}, UserId: {UserId}, SystemId: {SystemId}, RoleId: {RoleId}", mappingId, user.UserId, user.SystemId, user.RoleId);

            // 驗證路由參數：mappingId 不可為 null 或小於 0
            if (mappingId == null || mappingId < 0)
            {
                Log.Warning("[UsersController] 無效的權限對應 ID: {MappingId}", mappingId);
                return Failure("INVALID_MAPPING_ID", "無效ID", 400);
            }

            Log.Debug("[UsersController] 調用 MemberPermissionService.CheckMemberPermissionExistAsync");
            bool exist = await _memberPermissionService.CheckMemberPermissionExistAsync(user);

            if (exist)
            {
                Log.Warning("[UsersController] 嘗試更新為已存在的使用者權限: UserId={UserId}, SystemId={SystemId}, RoleId={RoleId}", user.UserId, user.SystemId, user.RoleId);
                return Failure("PERMISSION_ALREADY_EXISTS", "該使用者已擁有相同角色權限", 409);
            }

            Log.Debug("[UsersController] 調用 MemberPermissionService.UpdateMemberPermissionRoleAsync, MappingId: {MappingId}", mappingId);
            bool result = await _memberPermissionService.UpdateMemberPermissionRoleAsync(mappingId, user);


            if (!result)
            {
                Log.Warning("[UsersController] 找不到指定的使用者權限: MappingId={MappingId}", mappingId);
                return Failure("User_NOT_FOUND", "找不到指定的Id或是使用者", 404);
            }

            Log.Debug("[UsersController] 成功更新使用者權限: MappingId={MappingId}, UserId={UserId}, SystemId={SystemId}, RoleId={RoleId}", mappingId, user.UserId, user.SystemId, user.RoleId);
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
        [Authorize(Policy = "會員系統_系統管理員")]

        [HttpDelete("/api/users/{mappingId}/roles")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [Tags("會員管理")]
        public async Task<IActionResult> DeleteRole(int? mappingId)
        {
            Log.Debug("[UsersController] DeleteRole DELETE - Entry, MappingId: {MappingId}", mappingId);

            // 驗證路由參數：mappingId 不可為 null
            if (mappingId == null)
            {
                Log.Warning("[UsersController] 刪除使用者權限失敗，無效的權限對應 ID: {MappingId}", mappingId);
                return Failure("INVALID_MAPPING_ID", "無效ID", 400);
            }

            Log.Debug("[UsersController] 調用 MemberPermissionService.DeleteMemberPermissionRoleAsync, MappingId: {MappingId}", mappingId);
            bool result = await _memberPermissionService.DeleteMemberPermissionRoleAsync(mappingId);

            if (!result)
            {
                Log.Warning("[UsersController] 找不到指定的使用者權限，刪除失敗: MappingId={MappingId}", mappingId);
                return Failure("PERMISSION_NOT_FOUND", "找不到指定的權限", 404);
            }

            Log.Debug("[UsersController] 成功刪除使用者權限: MappingId={MappingId}", mappingId);
            return NoContent();
        }

        /// <summary>
        /// 取得前台領養寵物列表
        /// </summary>
        /// <returns>領養寵物列表 JSON</returns>
        /// <response code="200">成功取得領養寵物列表</response>
        [Authorize(Policy = "寵物系統_一般成員")]
        [HttpGet("/api/users/pet/adoption")]
        [ProducesResponseType(typeof(List<PetAdoptionDTO>), StatusCodes.Status200OK)]
        [Tags("寵物領養")]
        public async Task<IActionResult> GetAdoptionPets()
        {
            Log.Debug("[UsersController] GetAdoptionPets GET - Entry");

            Log.Debug("[UsersController] 調用 PetAdoptionService.GetAdoptionPetsAsync");
            var pets = await _petAdoptionService.GetAdoptionPetsAsync();

            Log.Debug("[UsersController] 取得寵物清單成功, 共 {Count} 隻", pets.Count);
            return Success(pets, "Success", 200);
        }

        /// <summary>
        /// 刊登送養寵物
        /// </summary>
        /// <param name="dto">刊登送養資料</param>
        /// <returns>創建成功的寵物資料</returns>
        /// <response code="200">成功刊登送養寵物</response>
        [Authorize(Policy = "會員系統_普通管理員")]
        [HttpPost("/api/users/pet/adoption")]
        [ProducesResponseType(typeof(PetAdoptionDTO), StatusCodes.Status200OK)]
        [Tags("寵物領養")]
        public async Task<IActionResult> CreateAdoptionPet([FromBody] PetCreateDto dto)
        {
            Log.Debug("[UsersController] CreateAdoptionPet POST - Entry");

            Log.Debug("[UsersController] 調用 PetAdoptionService.CreateAdoptionPetAsync");
            var createdPet = await _petAdoptionService.CreateAdoptionPetAsync(dto);

            Log.Debug("[UsersController] 刊登送養寵物成功, 名稱: {Name}, PetId: {PetId}", createdPet.Name, createdPet.PetId);
            return Success(createdPet, "成功刊登送養寵物", 200);
        }

        /// <summary>
        /// 取得目前登入會員的所有寵物健康護照清單
        /// </summary>
        [Authorize(Policy = "寵物系統_一般成員")]
        [HttpGet("/api/users/pet/passports")]
        [ProducesResponseType(typeof(List<PetPassportDisplayDto>), StatusCodes.Status200OK)]
        [Tags("寵物健康護照")]
        public async Task<IActionResult> GetPetPassports()
        {
            Log.Debug("[UsersController] GetPetPassports GET - Entry");

            // 假設從 Token Claims 取出目前登入者的 UserId，此處以模擬 1 替代，實務上改用 User.FindFirstClaim 或現有機制
            int currentUserId = 1;

            var passports = await _petPassportService.GetPetPassportsAsync(currentUserId);
            Log.Debug("[UsersController] 取得寵物健康護照成功，共 {Count} 筆", passports.Count);

            return Success(passports, "Success", 200);
        }

        /// <summary>
        /// 取得特定單筆寵物健康護照明細以供編輯
        /// </summary>
        [Authorize(Policy = "寵物系統_一般成員")]
        [HttpGet("/api/users/pet/passport/{id}")]
        [ProducesResponseType(typeof(PetPassportDetailDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [Tags("寵物健康護照")]
        public async Task<IActionResult> GetPassportDetail(int id)
        {
            Log.Debug("[UsersController] GetPassportDetail GET - Id: {Id}", id);
            int currentUserId = 1;

            var detail = await _petPassportService.GetPassportDetailAsync(id, currentUserId);
            if (detail == null)
            {
                Log.Warning("[UsersController] 找不到指定的寵物護照紀錄，Id: {Id}", id);
                return Failure("PASSPORT_NOT_FOUND", "找不到指定的健康護照紀錄", 404);
            }

            return Success(detail, "Success", 200);
        }

        /// <summary>
        /// 變更/儲存特定寵物健康護照內容
        /// </summary>
        [Authorize(Policy = "寵物系統_一般成員")]
        [HttpPut("/api/users/pet/passport/{id}")]
        [ProducesResponseType(typeof(bool), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [Tags("寵物健康護照")]
        public async Task<IActionResult> UpdatePassport(int id, [FromBody] PetPassportUpsertDto dto)
        {
            Log.Debug("[UsersController] UpdatePassport PUT - Id: {Id}", id);
            int currentUserId = 1;

            var isUpdated = await _petPassportService.UpdatePassportAsync(id, dto, currentUserId);
            if (!isUpdated)
            {
                return Failure("PASSPORT_UPDATE_FAILED", "修改失敗，紀錄不存在或無權限", 404);
            }

            Log.Debug("[UsersController] 更新寵物健康護照成功，Id: {Id}", id);
            return Success(true, "更新成功", 200);
        }

        /// <summary>
        /// 新增一筆毛孩健康護照紀錄
        /// </summary>
        [Authorize(Policy = "寵物系統_一般成員")]
        [HttpPost("/api/users/pet/passport")]
        [ProducesResponseType(typeof(PetPassportDetailDto), StatusCodes.Status200OK)]
        [Tags("寵物健康護照")]
        public async Task<IActionResult> CreatePassport([FromBody] PetPassportUpsertDto dto)
        {
            Log.Debug("[UsersController] CreatePassport POST - PetId: {PetId}", dto.PetId);
            int currentUserId = 1;

            var createdDetail = await _petPassportService.CreatePassportAsync(dto, currentUserId);
            Log.Debug("[UsersController] 建立健康護照紀錄成功，新護照標記碼: {Id}", createdDetail.Id);

            return Success(createdDetail, "建立成功", 200);
        }
        /// <summary>
        /// 依 UserId 取得對應的遊戲玩家資料
        /// </summary>
        /// <param name="userId">使用者 ID</param>
        /// <returns>玩家資料 (PlayerId、名稱、點數等)</returns>
        /// <response code="200">成功取得玩家資料</response>
        /// <response code="404">找不到對應的玩家</response>
        [Authorize(Policy = "遊戲系統_一般成員")]
        [AllowAnonymous]
        [HttpGet("{userId}/player-profile")]
        [ProducesResponseType(typeof(PlayerListDTO), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [Tags("會員管理")]
        public async Task<IActionResult> GetPlayerByUserId(int userId)
        {
            Log.Debug("[UsersController] GetPlayerByUserId GET - Entry, UserId: {UserId}", userId);

            try
            {
                var playerData = await _playerService.GetPlayerByUserIdAsync(userId);

                if (playerData == null)
                {
                    Log.Warning("[UsersController] 找不到對應的玩家, UserId: {UserId}", userId);
                    return Failure("PLAYER_NOT_FOUND", "找不到對應的玩家資料", 404);
                }

                Log.Debug("[UsersController] 成功取得玩家資料, UserId: {UserId}, PlayerId: {PlayerId}", userId, playerData.PlayerId);
                return Success(playerData, "Success", 200);
            }
            catch (Exception ex)
            {
                Log.Error("[UsersController] 取得玩家資料出錯, UserId: {UserId}, Error: {Error}", userId, ex.Message);
                return Failure("ERROR", "取得玩家資料失敗", 500);
            }


        }


        /// <summary>
        /// 首頁使用：按照篩選條件取得全站所有文章
        /// </summary>
        [AllowAnonymous]
        [HttpGet("articles")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [Tags("社群管理")]
        public async Task<IActionResult> GetAllArticleList([FromQuery] ArticleQueryDTO queryDto)
        {
            // 首頁撈取時，不限定單一使用者（除非前端特別傳入 ?userId=xxx 撈特定人的公開文）
            var result = await _articleService.GetAllArticlesAsync(
                status: queryDto.Status,     // 通常首頁只撈公開的 (Status = 1)
                isActive: queryDto.IsActive, // 通常首頁只撈未刪除的 (IsActive = true)
                userId: queryDto.UserId
            );
            return Success(result, "取得全站文章成功", 200);
        }

        // 取得特定使用者的所有文章
        /// <summary>
        /// 取得特定使用者的所有文章
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="queryDto"></param>
        /// <returns></returns>
        [Authorize(Policy = "社群系統_一般成員")]
        [HttpGet("users/{userId}/articles")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [Tags("社群管理")]
        public async Task<IActionResult> ArticleList([FromRoute] int userId, [FromQuery] ArticleQueryDTO queryDto)
        {
            var result = await _articleService.GetAllArticlesAsync(
                status: queryDto.Status,
                isActive: queryDto.IsActive,
                userId: userId
                );
            return Success(result, "取得使用者文章成功", 200);
        }

        /// <summary>
        /// 提供給 Quill 編輯器上傳圖片的專用接口
        /// </summary>
        /// <param name="file">前端傳入的圖片檔案</param>
        /// <returns>回傳 JSON 格式的圖片網址</returns>
        [Authorize(Policy = "社群系統_一般成員")]
        [HttpPost("community/upload")]
        [Tags("社群管理")]
        public async Task<IActionResult> Upload([FromForm] IFormFile file)
        {
            // 1. 防呆：確保有收到檔案
            if (file == null || file.Length == 0)
            {
                Log.Warning("[UsersController] Upload - 未接收到檔案或檔案大小為 0");
                return Failure("FILE_EMPTY", "未接收到檔案", 400);
            }

            try
            {
                Log.Debug("[UsersController] Upload - 開始呼叫 FileService 儲存圖片");

                // 2. 呼叫 Service 進行驗證與安全存檔（此時 FileService 內部已改用 await using 與正確路徑）
                string imageUrl = await _fileService.SaveImageForQuillAsync(file, HttpContext.Request);

                // 3. 檢查 Service 回傳結果
                if (string.IsNullOrEmpty(imageUrl))
                {
                    Log.Warning("[UsersController] Upload - 圖片儲存失敗，副檔名不符或儲存程序異常");
                    return Failure("INVALID_FILE_FORMAT", "檔案格式不正確或圖片儲存失敗", 400);
                }

                Log.Debug("[UsersController] Upload - 圖片上傳成功，網址: {Url}", imageUrl);

                // 4. 成功後回傳 Quill 認得的物件格式 { url: "https://..." }
                return Success(new { url = imageUrl }, "Success", 200);
            }
            catch (Exception ex)
            {

                // 這樣做能確保後端發生任何不可預期的內部錯誤時，只會優雅地回傳 500，而「黑色的命令提示字元視窗」絕對不會自己關掉！
                Log.Error(ex, "[UsersController] Upload - 圖片上傳期間發生未預期致命錯誤: {Message}", ex.Message);

                return Failure("INTERNAL_ERROR", "伺服器內部錯誤，圖片上傳失敗", 500);
                
            }
        }

        //新增文章
        /// <summary>
        /// 新增文章
        /// </summary>
        /// <param name="articleDto"></param>
        /// <returns></returns>
        /// <response code="400">資料驗證失敗</response>
        /// <response code="200">文章建立成功</response>
        /// <response code="500">伺服器內部錯誤</response>
        [Authorize(Policy = "社群系統_一般成員")]
        [HttpPost("articles")]
        [Tags("社群管理")]

        public async Task<IActionResult> Article([FromBody] ArticleSaveDTO articleDto)
        {
            if (!ModelState.IsValid)
            {
                return Failure("VALIDATION_ERROR", "資料驗證失敗", 400);
            }
            try
            {
                var result = await _articleService.CreateArticleAsync(articleDto);
                return Success(result, "文章建立成功", 200);
                //**跳轉到文章詳細頁面
                
            }
            catch (Exception ex)
            {
                return Failure("INTERNAL_ERROR", "伺服器內部錯誤", 500);
            }
        }

        //=====取得文章詳細(文章id)=====
        /// <summary>
        /// 取得文章詳細
        /// </summary>
        /// <param name="id">文章 ID</param>
        /// <returns>文章詳細資料</returns>
        /// <response code="404">找不到文章</response>
        /// <response code="200">取得文章詳細成功</response>
        /// <response code="500">伺服器內部錯誤</response>
        [AllowAnonymous]
        [HttpGet("articles/{id}")]
        [Tags("社群管理")]
        public async Task<IActionResult> GetArticleDetail(int id)
        {
            try
            {
                var result = await _articleService.GetArticleDetailAsync(id);

                if (result == null)
                {
                    return Failure("ARTICLE_NOT_FOUND", "找不到文章", 404);
                }

                return Success(result, "取得文章詳細成功", 200);
            }
            catch (Exception ex)
            {
                return Failure("INTERNAL_ERROR", ex.Message, 500);
            }
        }



    }
}
