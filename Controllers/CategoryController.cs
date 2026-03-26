using Microsoft.AspNetCore.Mvc;
using PawsPort.Models;
using PawsPort.ViewModels;

namespace PawsPort.Controllers
{
    public class CategoryController : Controller
    {
        //public IActionResult Index()
        //{
        //    return View();
        //}


        public IActionResult CategoryList(KeywordViewModel vm)
        {
            PetDbContext db = new PetDbContext();

            IEnumerable<Category> datas = null; //宣告一個變數來存放查詢結果

            if (string.IsNullOrEmpty(vm.txtKeyword))
            {
                datas = from p in db.Categories
                        where p.IsExist
                        select p;
            }
            else
            {
                datas = db.Categories.Where(a => a.IsExist
                && (a.CategoryName.Contains(vm.txtKeyword)
                )); //根據搜尋條件查詢類別
            }

            return View(datas);
        }
    }
}
