using Microsoft.AspNetCore.Mvc;
using PawsPort.Models;
using PawsPort.ViewModels;

namespace PawsPort.Controllers
{
    public class CategoryController : Controller
    {


        public IActionResult CategoryList(CategoryListViewModel vm)
        {
            using (PetDbContext db = new PetDbContext())
            {
                //抓出所有存在的分類
                var RawDatas = db.Categories.Where(c => c.IsExist == true);
              
                //把分類底下所有的文章包裝起來，統計貼文數
                var AllItems = RawDatas.Select(c => new CategoryItemViewModel
                {
                    CategoryId = c.CategoryId,
                    CategoryName = c.CategoryName,
                    CategoryDescription = c.CategoryDescription,
                    ParentID = c.ParentId,
                    Level = c.Level,
                    SortOrder = c.SortOrder,
                    CreateAt = c.CreateAt,
                    ArticleCount = db.Articles.Count(a => a.CategoryId == c.CategoryId && a.IsExist == true),
                });

                if (!string.IsNullOrEmpty(vm.TxtKeyword)) //有關鍵字的話，父子分類都撈出來
                {
                    AllItems = AllItems.Where(a => a.CategoryName.Contains(vm.TxtKeyword));
                }

                var Parents = AllItems.Where(a => a.ParentID == null).ToList(); //從撈出的分類抓出父分類

                //定義一組色系
                string[] colorSchemes = { "primary", "success", "info", "warning", "danger", "secondary" };

                for(int i=0; i < Parents.Count(); i++)
                {
                    Parents[i].ColorClass = colorSchemes[i % colorSchemes.Length];
                    // 抓出屬於這個父分類的子分類
                    Parents[i].ChildCategories = AllItems.Where(c => c.ParentID == Parents[i].CategoryId).ToList();
                }

                // 5. 排序與分頁 (針對父分類進行分頁)
                vm.TotalCount = Parents.Count;
                vm.ParentCategories = Parents
                    .OrderByDescending(p => p.CreateAt) // 範例：按時間排
                    .Skip((vm.CurrentPage - 1) * vm.PageSize)
                    .Take(vm.PageSize)
                    .ToList();

                return View(vm);
            }

            //首先要有關鍵字查詢功能，大分類和小分類都要撈出來
            //不同分類要顯示不同顏色，同一組分類(父子)顯示同一種顏色(父較深子較淺)，不同組之間要有不同顏色
            //要有按照新增時間/該分類下貼文數量的排序
            //要有分頁功能，希望能配合cshtml的分頁顯示下拉選單來調整分頁裡顯示幾筆資料
            //父子分類希望作成collapsable card，外面是父分類，點開裡面顯示子分類

        }


        public IActionResult CreateCategory()
        {
            using (PetDbContext db = new PetDbContext())
            {
                // 抓出可以當作「上級」的分類（排除掉已經是第三層的，因為它不能再有子類）
                var Parents = db.Categories
                    .Where(c => c.IsExist && c.Level < 2)
                    .OrderBy(c => c.Level)
                    .ThenBy(c => c.SortOrder)
                    .ToList();

                ViewBag.ParentList = Parents;
            }
            return View();
        }

        [HttpPost]
        public IActionResult CreateCategory(Category p)
        {
            if (ModelState.IsValid)
            {
                using (PetDbContext db = new PetDbContext())
                {
                    p.IsExist = true;
                    p.CreateAt = DateTime.Now;
                    p.LastEditTime = DateTime.Now;

                    // 判斷層級：有選父類就是 Level 1，沒選就是 Level 0
                    p.Level = p.ParentId.HasValue ? 1 : 0;

                    db.Categories.Add(p);
                    db.SaveChanges();
                    return RedirectToAction("CategoryList");
                }
            }
            // 失敗時重新載入
            return CreateCategory(p);

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
