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

                for (int i = 0; i < Parents.Count(); i++)
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
            if (id == null) return RedirectToAction("CategoryList");

            using (PetDbContext db = new PetDbContext())
            {
                Category x = db.Categories.FirstOrDefault(p => p.CategoryId == id && p.IsExist);
                if (x == null) return RedirectToAction("CategoryList");

                // 修正後的 LINQ：只要是 Level 0 的都抓出來，排除自己（避免自己變自己的父類）
                ViewBag.ParentList = db.Categories
                    .Where(c => c.IsExist && c.Level == 0 && c.CategoryId != id)
                    .OrderBy(c => c.SortOrder)
                    .ToList();

                return View(x);
            }
        }

        [HttpPost]
        public IActionResult EditCategory(Category uiCategory)
        {
            // 注意：這裡如果 ModelState 因為某些暫時不用的欄位失效，可以直接移除 if 檢查或針對性處理
            using (PetDbContext db = new PetDbContext())
            {
                Category dbCategory = db.Categories.FirstOrDefault(p => p.CategoryId == uiCategory.CategoryId);

                if (dbCategory != null)
                {
                    dbCategory.CategoryName = uiCategory.CategoryName;
                    dbCategory.CategoryDescription = uiCategory.CategoryDescription;
                    dbCategory.SortOrder = uiCategory.SortOrder;
                    dbCategory.LastEditTime = DateTime.Now;

                    // 核心修正：明確處理 ParentId 與 Level
                    if (uiCategory.ParentId.HasValue && uiCategory.ParentId > 0)
                    {
                        dbCategory.ParentId = uiCategory.ParentId;
                        dbCategory.Level = 1; // 有選父類，強設為子分類
                    }
                    else
                    {
                        dbCategory.ParentId = null;
                        dbCategory.Level = 0; // 沒選父類，強設為主分類
                    }

                    db.SaveChanges();
                    return RedirectToAction("CategoryList");
                }
            }
            return RedirectToAction("CategoryList"); ;
        }

        public IActionResult DeleteCategory(int? id)
        {
            using (PetDbContext db = new PetDbContext())
            {
                // 1. 找出要刪除的分類
                Category x = db.Categories.FirstOrDefault(p => p.CategoryId == id);

                if (x != null)
                {
                    // 軟刪除目標分類
                    x.IsExist = false;
                    x.LastEditTime = DateTime.Now; // 紀錄最後修改時間

                    // 2. 【進階處理】如果這是父分類 (Level 0)，把底下的子分類也一起軟刪除
                    if (x.Level == 0)
                    {
                        var children = db.Categories.Where(c => c.ParentId == x.CategoryId && c.IsExist);
                        foreach (var child in children)
                        {
                            child.IsExist = false;
                            child.LastEditTime = DateTime.Now;
                        }
                    }

                    db.SaveChanges();
                }
            }
            return RedirectToAction("CategoryList");
        }

    }

}