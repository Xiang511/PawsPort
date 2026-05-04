using PawsPort.Dtos;
using PawsPort.Models;
using Microsoft.EntityFrameworkCore;

namespace PawsPort.Services
{
    public class CategoryService
    {
        //注入資料庫
        private readonly PetDbContext _context;

        public CategoryService(PetDbContext context)
        {
            _context = context;
        }


        //分類列表


        //新增分類
        public async Task<int> CreateCategoryAsync(CategorySaveDTO categorySaveDTO)
        {
            //input:分類名稱、分類描述、父分類Id、排序順序
            //output:新分類Id

            int TargetLevel = 0; //預設為大分類
            int FinalSort = 0;


            //如果parentid有值=是子分類
            if (categorySaveDTO.ParentId.HasValue)
            {
                //去撈parent id的資料
                var ParentCategory = await _context.Categories
                    .FirstOrDefaultAsync(p => p.CategoryId == categorySaveDTO.ParentId && p.IsExist == true);

                //檢查parent id是否存在，且level為0(最上層)
                if (ParentCategory == null) throw new Exception("找不到指定的父分類");
                if (ParentCategory.Level >= 2) throw new Exception("目前僅支援三層分類結構，該分類無法再擁有子分類");

                //驗證ok，此分類的level基於父分類level+1
                TargetLevel = ParentCategory.Level + 1;
            }

            //自動排序
            if (categorySaveDTO.SortOrder == 0)
            {
                //沒有傳入排序的話
                //找同一個parent id下有幾個子分類，取最大值並+1
                //沒資料(null)的話，值就會=0
                 var MaxSort = await _context.Categories.Where(m => m.ParentId == categorySaveDTO.ParentId)
                    .Select(m => (int?)m.SortOrder)
                    .MaxAsync();
                FinalSort = MaxSort.HasValue ? MaxSort.Value+1 : 0;
            }
            else
            {
                //有輸入排序
                FinalSort = categorySaveDTO.SortOrder;
            }

            //建立新的分類實體，並設定屬性
            var CategoryEntity = new Category
            {
                CategoryName = categorySaveDTO.CategoryName,
                CategoryDescription = categorySaveDTO.CategoryDescription,
                ParentId = categorySaveDTO.ParentId, //或是選擇的父分類Id
                Level = TargetLevel, //使用上列計算出來的level
                SortOrder = FinalSort, //可以根據需求設定排序順序
                IsExist = true,
                CreateAt = DateTime.UtcNow,
                LastEditTime = DateTime.UtcNow
            };
            //將分類實體添加到資料庫
            _context.Categories.Add(CategoryEntity);
            //保存更改到資料庫，拿到CategoryId
            await _context.SaveChangesAsync();

            return CategoryEntity.CategoryId;

        }


        //編輯分類



        //刪除分類


    }
}
