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
    [Tags("社群管理")]
    public class ArticleController : ApiControllerBase
    {
        //注入資料庫和service
        private readonly PetDbContext _context;
        private readonly ArticleService _articleService;

        private readonly IWebHostEnvironment _Env = null;

        public ArticleController(IWebHostEnvironment p, PetDbContext context, ArticleService articleService)
        {
            _Env = p;
            _context = context;
            _articleService = articleService;
        }

        //取得所有文章
        /// <summary>
        /// 按照篩選條件取得所有文章
        /// </summary>
        /// <param name="queryDto">篩選條件</param>
        /// <returns></returns>
        /// <response code="200">取得所有文章成功</response>
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [Tags("社群管理/貼文管理")]

        public async Task<IActionResult> ArticleList([FromQuery] ArticleQueryDTO queryDto)
        {
            var result = await _articleService.GetAllArticlesAsync(
                status: queryDto.Status,
                isActive: queryDto.IsActive,
                userId: queryDto.UserId
                );
            return Success(result, "取得所有文章成功", 200);

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
        [HttpPost]
        [Tags("社群管理/貼文管理")]

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

        //編輯文章
        /// <summary>
        /// 編輯文章
        /// </summary>
        /// <param name="id"></param>
        /// <param name="articleDto"></param>
        /// <returns></returns>
        /// <response code="400">無效的文章編號</response>
        /// <response code="404">找不到該文章</response>
        /// <response code="200">文章更新成功</response>
        [HttpPut("{id}")]
        [Tags("社群管理/貼文管理")]
        public async Task<IActionResult> Article(int id, [FromBody] ArticleSaveDTO articleDto)
        {
            if (id <= 0)
            {
                return Failure("INVALID_ID", "無效的文章編號", 400);
            }

            //如果一致，呼叫service更新文章
            var result = await _articleService.UpdateArticleAsync(id, articleDto);
            if (result == null)
            {
                //回傳找不到該編號文章
                return Failure("ARTICLE_NOT_FOUND", "找不到該文章", 404);
            }
            else
            {
                return Success(result, "文章更新成功", 200);
                //**跳轉到文章詳細頁面
            }

        }



        //軟刪除文章
        /// <summary>
        /// 軟刪除文章
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        /// <response code="400">無效的文章編號</response>
        /// <response code="404">找不到該文章</response>
        /// <response code="200">文章刪除成功</response>
        [HttpPatch("{id}")]
        [Tags("社群管理/貼文管理")]
        public async Task<IActionResult> Delete(int id)
        {
            //檢查id是否有效
            if (id <= 0) return Failure("INVALID_ID", "無效的文章編號", 400);

            //有效的話呼叫service
            var result = await _articleService.DeleteArticleAsync(id);
            //若service回傳false，代表找不到該文章
            //若service回傳true，代表刪除成功
            if (result == false)
            {
                //回傳找不到該編號文章
                return Failure("ARTICLE_NOT_FOUND", "找不到該文章", 404);
            }
            else
            {
                return Success(result, "文章刪除成功", 200);
            }
        }

    }
}
