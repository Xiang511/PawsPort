using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using PawsPort.Dtos;
using PawsPort.Models;
using PawsPort.Services;
using PawsPort.ViewModels;
using Serilog;
using Serilog.Events;

namespace PawsPort.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Produces("application/json")]
    [Tags("留言管理")]
    public class CommentController : ApiControllerBase
    {
        //注入資料庫和service
        private readonly PetDbContext _context;
        private readonly CommentService _commentService;

        private readonly IWebHostEnvironment _Env = null;

        public CommentController(IWebHostEnvironment p, PetDbContext context, CommentService commentService)
        {
            _Env = p;
            _context = context;
            _commentService = commentService;
        }

        //留言列表

        //新增留言
        /// <summary>
        /// 新增留言
        /// </summary>
        /// <param name="commentSaveDTO"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<IActionResult> Comment(CommentSaveDTO commentSaveDTO)
        {
            if (!ModelState.IsValid)
            {
                return Failure("VALIDATION_ERROR", "資料驗證失敗", 400);
            }
            try
            {
                var result = await _commentService.CreateCommentAsync(commentSaveDTO);
                return Success(result, "留言建立成功", 200);
                //**跳轉到文章詳細頁面
            }
            catch (Exception ex)
            {
                return Failure("INTERNAL_ERROR", ex.Message, 500);
            }
        }

        //軟刪除留言
        /// <summary>
        /// 軟刪除留言
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            //檢查id是否正常
            if (id <= 0) return Failure("INVALID_ID", "無效的留言編號", 400);
            try
            {
                var result = await _commentService.DeleteCommentAsync(id);
                if (result == false)
                {
                    return Failure("COMMENT_NOT_FOUND", "找不到該留言", 404);
                }
                else
                {
                    return Success(result, "留言刪除成功", 200);
                }
            }
            catch (InvalidOperationException ex)
            {
                // 專門處理「被屏蔽而禁止刪除」的情況
                return Failure("FORBIDDEN_ACTION", ex.Message, 403);
            }
            catch (Exception)
            {
                return Failure("INTERNAL_ERROR", "伺服器錯誤", 500);
            }
        }

        //管理留言
    }
}
