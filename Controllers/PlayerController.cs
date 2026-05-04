using Azure;
using Microsoft.AspNetCore.Mvc;
using PawsPort.Models;
using PawsPort.Services;

namespace PawsPort.Controllers
{
    public class PlayerController : ApiControllerBase
    {
        //注入資料庫和service(防止檢查)
        private readonly PetDbContext _context;
        private readonly CategoryService _categoryService;

        public PlayerController(PetDbContext context, CategoryService categoryService)
        {
            _context = context;
            _categoryService = categoryService;
        }
        //防止檢查分隔線
    }
}
