using Microsoft.AspNetCore.Mvc;
using PawsPort.Models;

namespace PawsPort.Controllers.Api
{
    /// <summary>
    /// 會員管理 API
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    [Produces("application/json")]
    public class MemberApiController : ControllerBase
    {
        private readonly PetDbContext _context;
        private readonly ILogger<MemberApiController> _logger;

        public MemberApiController(PetDbContext context, ILogger<MemberApiController> logger)
        {
            _context = context;
            _logger = logger;
        }

        /// <summary>
        /// 取得所有會員列表（未刪除）
        /// </summary>
        /// <returns>會員列表 JSON</returns>
        /// <response code="200">成功取得會員列表</response>
        [HttpGet]
        [ProducesResponseType(typeof(IEnumerable<UserTable>), StatusCodes.Status200OK)]
        public ActionResult<IEnumerable<UserTable>> GetMembers()
        {
            var members = _context.UserTables
                .Where(x => x.DeleteDay == null)
                .ToList();

            _logger.LogInformation("取得 {Count} 筆會員資料", members.Count);
            return Ok(members);
        }

        /// <summary>
        /// 依 ID 取得單一會員
        /// </summary>
        /// <param name="id">會員 ID</param>
        /// <returns>會員資料 JSON</returns>
        /// <response code="200">成功取得會員資料</response>
        /// <response code="404">找不到指定的會員</response>
        [HttpGet("{id}")]
        [ProducesResponseType(typeof(UserTable), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public ActionResult<UserTable> GetMember(int id)
        {
            var member = _context.UserTables
                .Where(m => m.UserId == id && m.DeleteDay == null)
                .FirstOrDefault();

            if (member == null)
            {
                _logger.LogWarning("找不到會員 ID: {UserId}", id);
                return NotFound(new { message = $"找不到會員 ID: {id}" });
            }

            return Ok(member);
        }

        /// <summary>
        /// 建立新會員
        /// </summary>
        /// <param name="user">會員資料</param>
        /// <returns>新建立的會員資料</returns>
        /// <response code="201">成功建立會員</response>
        /// <response code="400">請求資料格式錯誤</response>
        [HttpPost]
        [ProducesResponseType(typeof(UserTable), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public ActionResult<UserTable> CreateMember([FromBody] UserTable user)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            user.CreatedAt = DateTime.Now;
            user.UpdatedAt = DateTime.Now;
            _context.UserTables.Add(user);
            _context.SaveChanges();

            _logger.LogInformation("新增會員: {UserName} (ID: {UserId})", user.Name, user.UserId);

            return CreatedAtAction(nameof(GetMember), new { id = user.UserId }, user);
        }

        /// <summary>
        /// 更新會員資料
        /// </summary>
        /// <param name="id">會員 ID</param>
        /// <param name="user">更新的會員資料</param>
        /// <returns>無內容</returns>
        /// <response code="204">成功更新會員</response>
        /// <response code="404">找不到指定的會員</response>
        /// <response code="400">請求資料格式錯誤</response>
        [HttpPut("{id}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public IActionResult UpdateMember(int id, [FromBody] UserTable user)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var dbUser = _context.UserTables
                .Where(m => m.UserId == id && m.DeleteDay == null)
                .FirstOrDefault();

            if (dbUser == null)
            {
                return NotFound(new { message = $"找不到會員 ID: {id}" });
            }

            dbUser.Name = user.Name;
            dbUser.Photo = user.Photo;
            dbUser.Job = user.Job;
            dbUser.Phone = user.Phone;
            dbUser.City = user.City;
            dbUser.Note = user.Note;
            dbUser.Birthday = user.Birthday;
            dbUser.Status = user.Status;
            dbUser.HasPriorExp = user.HasPriorExp;
            dbUser.Point = user.Point;
            dbUser.IsSubscribe = user.IsSubscribe;
            dbUser.IsVerify = user.IsVerify;
            dbUser.UpdatedAt = DateTime.Now;

            _context.SaveChanges();

            _logger.LogInformation("更新會員: {UserName} (ID: {UserId})", dbUser.Name, dbUser.UserId);

            return NoContent();
        }

        /// <summary>
        /// 軟刪除會員
        /// </summary>
        /// <param name="id">會員 ID</param>
        /// <returns>無內容</returns>
        /// <response code="204">成功刪除會員</response>
        /// <response code="404">找不到指定的會員</response>
        [HttpDelete("{id}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public IActionResult DeleteMember(int id)
        {
            var member = _context.UserTables
                .Where(m => m.UserId == id && m.DeleteDay == null)
                .FirstOrDefault();

            if (member == null)
            {
                return NotFound(new { message = $"找不到會員 ID: {id}" });
            }

            member.DeleteDay = DateTime.Now;
            member.UpdatedAt = DateTime.Now;
            _context.SaveChanges();

            _logger.LogWarning("刪除會員: {UserName} (ID: {UserId})", member.Name, member.UserId);

            return NoContent();
        }

        /// <summary>
        /// 取得會員統計資料
        /// </summary>
        /// <returns>統計資料 JSON</returns>
        /// <response code="200">成功取得統計資料</response>
        [HttpGet("statistics")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public ActionResult GetStatistics()
        {
            var memberCount = _context.UserTables.Count(x => x.DeleteDay == null);

            DateTime today = DateTime.Now;
            DateTime startOfMonth = new DateTime(today.Year, today.Month, 1);
            DateTime startOfNextMonth = startOfMonth.AddMonths(1);
            var memberMonthSignUp = _context.UserTables
                .Count(u => u.CreatedAt >= startOfMonth && u.CreatedAt < startOfNextMonth);

            var memberVerify = _context.UserTables.Count(x => x.IsVerify == true && x.DeleteDay == null);
            var verifyPercentage = memberCount > 0 ? ((float)memberVerify / memberCount) * 100 : 0;

            var memberRss = _context.UserTables.Count(x => x.IsSubscribe == true && x.DeleteDay == null);

            var statistics = new
            {
                totalMembers = memberCount,
                monthlySignUps = memberMonthSignUp,
                verifiedCount = memberVerify,
                verifyPercentage = verifyPercentage.ToString("F1"),
                subscribedCount = memberRss
            };

            return Ok(statistics);
        }
    }
}
