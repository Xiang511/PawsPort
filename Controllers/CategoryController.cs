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
    public class CategoryController : ApiControllerBase
    {
        //注入資料庫和service
        private readonly PetDbContext _context;
        private readonly CategoryService _categoryService;

        public CategoryController(PetDbContext context, CategoryService categoryService)
        {
            _context = context;
            _categoryService = categoryService;
        }

        



        //新增分類
        /// <summary>
        /// 新增分類
        /// </summary>
        /// <param name="categorySaveDTO"></param>
        /// <returns></returns>
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [HttpPost]
        public async Task<IActionResult> Category([FromBody] CategorySaveDTO categorySaveDTO)
        {
            if (!ModelState.IsValid)
            {
                return Failure("VALIDATION_ERROR", "資料驗證失敗", 400);
            }
            try
            {
                var result = await _categoryService.CreateCategoryAsync(categorySaveDTO);
                return Success(result, "分類建立成功", 200);
                //**跳轉到分類管理頁面
            }
            catch (Exception ex)
            {
             
                return Failure("INTERNAL_ERROR", ex.Message, 500);
            }
        }

        /// <summary>
        /// 編輯分類
        /// </summary>
        /// <param name="id"></param>
        /// <param name="categorySaveDTO"></param>
        /// <returns></returns>
        [HttpPut("{id}")]
        public async Task<IActionResult> Category(int id,[FromBody]CategorySaveDTO categorySaveDTO)
        {
            if (!ModelState.IsValid)
            {
                return Failure("VALIDATION_ERROR", "資料驗證失敗", 400);
            }
            try
            {
                var result = await _categoryService.UpdateCategoryAsync(id,categorySaveDTO);
                return Success(result, "分類更新成功", 200);
                //**跳轉到分類管理頁面
            }
            catch (Exception ex)
            {
                return Failure("INTERNAL_ERROR", ex.Message, 500);
            }
        }


    }
}