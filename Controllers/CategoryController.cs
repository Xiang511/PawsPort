using Microsoft.AspNetCore.Mvc;
using PawsPort.Models;
using PawsPort.ViewModels;

namespace PawsPort.Controllers
{
    public class CategoryController : Controller
    {
 

        public IActionResult CategoryList(KeywordViewModel vm)
        {
            PetDbContext db = new PetDbContext();

            IEnumerable<Category> datas = null; //宣告一個變數來存放查詢結果

            if (string.IsNullOrEmpty(vm.TxtKeyword))
            {
                datas = from p in db.Categories
                        where p.IsExist
                        select p;
            }
            else
            {
                datas = db.Categories.Where(p => p.IsExist
                && (p.CategoryName.Contains(vm.TxtKeyword)
                )); //根據搜尋條件查詢類別
            }

            return View(datas);
        }


        public IActionResult CreateCategory()
        {
            return View();
        }

        [HttpPost]
        public IActionResult CreateCategory(Category p)
        {
            PetDbContext db = new PetDbContext();

            p.IsExist = true; //設定類別為存在狀態
            p.CreateAt = DateTime.Now; //設定類別的建立時間為目前時間
   
            db.Categories.Add(p);
            db.SaveChanges();
            return RedirectToAction("CategoryList");

        }

        public IActionResult EditCategory(int? id)
        {
            PetDbContext db = new PetDbContext();
            Category x = db.Categories.FirstOrDefault(p => p.CategoryId == id);
            if (x == null)
            {
                return RedirectToAction("CategoryList");
            }
            return View(x);
        }

        [HttpPost]
        public IActionResult EditCategory(Category uiCategory)
        {
            PetDbContext db = new PetDbContext(); //建立資料庫上下文
            Category dbCategory = db.Categories.FirstOrDefault(p => p.CategoryId == uiCategory.CategoryId); 
            //從資料庫中查找要編輯的類別
            if (dbCategory != null)
            {
                dbCategory.CategoryName = uiCategory.CategoryName; //更新類別名稱
                dbCategory.CategoryDescription = uiCategory.CategoryDescription; //更新類別描述
                dbCategory.ParentId = uiCategory.ParentId; //更新父類別ID
                dbCategory.Level = uiCategory.Level; //更新類別層級
                dbCategory.SortOrder = uiCategory.SortOrder; //更新類別排序
                dbCategory.LastEditTime = DateTime.Now; //更新最後編輯時間
                db.SaveChanges(); //保存更改到資料庫

            }
            return RedirectToAction("CategoryList");
        }

        public IActionResult DeleteCategory(int? id)
        {
            PetDbContext db = new PetDbContext();
            Category x = db.Categories.FirstOrDefault(p => p.CategoryId == id);
            if (x != null)
            {
                x.IsExist = false;
                db.SaveChanges();
            }
            return RedirectToAction("CategoryList");
        }

    }
}
