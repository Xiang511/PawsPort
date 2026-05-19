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


        //=====分類列表=====
        public async Task<List<CategoryListDTO>> GetCategoriesAsync()
        {
            var categories = await (from c in _context.Categories
                                        // 這裡使用 Left Join 連接自己，撈出父分類
                                    join p in _context.Categories on c.ParentId equals p.CategoryId into parentJoin
                                    from p in parentJoin.DefaultIfEmpty()
                                    select new CategoryListDTO
                                    {
                                        // 如果沒有父分類，就給它空字串或 null
                                        CategoryId=c.CategoryId,
                                        ParentId= p != null ? p.ParentId : null,
                                        ParentCategoryName = p != null ? p.CategoryName : string.Empty,
                                        CategoryName = c.CategoryName
                                    }).ToListAsync();

            return categories;
        }

        //=====新增分類=====
        public async Task<int> CreateCategoryAsync(CategorySaveDTO categorySaveDTO)
        {
            //input:分類名稱、分類描述、父分類Id、排序順序
            //output:新分類Id

            int TargetLevel = 0; //預設為大分類

            //如果parentid沒有值(null):預設給0(最大分類)
            if (!categorySaveDTO.ParentId.HasValue)
            {
                TargetLevel = 0;
            }
            else
            {
                //如果parentid有值=是子分類
                //去撈parent id的資料
                var Parent = await _context.Categories
                    .FirstOrDefaultAsync(p => p.CategoryId == categorySaveDTO.ParentId && p.IsExist == true);

                //檢查parent id是否存在，且level >=2(最多只能到1)
                if (Parent == null) throw new Exception("找不到指定的父分類");
                if (Parent.Level >= 2) throw new Exception("目前僅支援三層分類結構，該分類無法再擁有子分類");

                //驗證ok，此分類的level基於父分類level+1
                TargetLevel = (Parent.Level ?? 0) + 1;
            }

            //自動排序
            int FinalSort = 0;
            if (!categorySaveDTO.SortOrder.HasValue)
            {
                //沒有傳入排序的話
                //MaxAsync:找同一個parent id下，sortorder欄位的最大值
                var MaxSort = await _context.Categories.Where(m => m.ParentId == categorySaveDTO.ParentId && m.IsExist == true)
                   .Select(m => (int?)m.SortOrder)
                   .MaxAsync();
                //如果maxsort=null，那排序=0(-1+1)，如果maxsort=0，那排序=1(0+1)
                FinalSort = (MaxSort ?? -1) + 1;
            }
            else
            {
                //有輸入排序
                FinalSort = categorySaveDTO.SortOrder.Value;
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


        //=====編輯分類=====
        public async Task<int> UpdateCategoryAsync(int id, CategorySaveDTO categorySaveDTO)
        {
            //先去撈看看有沒有對應id的分類
            var CategoryEntity = await _context.Categories.FirstOrDefaultAsync(c => c.CategoryId == id && c.IsExist == true);
            //沒有就回傳找不到
            if (CategoryEntity == null) throw new Exception("找不到指定的分類");

            //處理層級，避免人為任意修改
            int TargetLevel = 0;
            if (categorySaveDTO.ParentId.HasValue)
            {
                if (categorySaveDTO.ParentId == id) throw new Exception("父分類不能設定為分類自己本身");
                var Parent = await _context.Categories
                    .FirstOrDefaultAsync(p => p.CategoryId == categorySaveDTO.ParentId && p.IsExist == true);
                if (Parent == null) throw new Exception("找不到指定的父分類");
                if (Parent.Level >= 2) throw new Exception("分類層級過深，不支援此結構"); //最多層級到2，由於必須+1，父分類層級最多到1
                
                TargetLevel = (Parent.Level ?? 0) + 1;
            }

            //自動排序
            //如果沒有傳入排序，或換了父分類(實體與傳入不相等)，會自動排序到最尾
            bool NeedReSort = !categorySaveDTO.SortOrder.HasValue || CategoryEntity.ParentId != categorySaveDTO.ParentId;
            int FinalSort = 0;

            if (NeedReSort)
            {
                //沒有傳入排序的話
                //MaxAsync:找同一個parent id下，sortorder欄位的最大值
                //沒資料(null)的話，值就會=0
                var MaxSort = await _context.Categories.Where(m => m.ParentId == categorySaveDTO.ParentId && m.IsExist==true)
                   .Select(m => (int?)m.SortOrder)
                   .MaxAsync();
                //如果maxsort=null，那排序=0(-1+1)，如果maxsort=0，那排序=1(0+1)
                FinalSort = (MaxSort ?? -1) + 1;
            }
            else
            {
                //有輸入排序
                FinalSort = categorySaveDTO.SortOrder.Value;
            }

            //更新屬性
            CategoryEntity.CategoryName = categorySaveDTO.CategoryName;
            CategoryEntity.CategoryDescription = categorySaveDTO.CategoryDescription;
            CategoryEntity.ParentId = categorySaveDTO.ParentId;
            CategoryEntity.Level = TargetLevel;
            CategoryEntity.SortOrder = FinalSort;
            CategoryEntity.LastEditTime = DateTime.UtcNow;

            //儲存修改
            await _context.SaveChangesAsync();
            return CategoryEntity.CategoryId;
        }


        //=====刪除分類=====
        public async Task<bool> DeleteCategoryAsync(int id)
        {
            //撈對應id的分類
            var CategoryEntity = await _context.Categories.FirstOrDefaultAsync(c => c.CategoryId == id);
            
            //檢查有沒有該分類，或他是否已被軟刪除
            if(CategoryEntity == null||CategoryEntity.IsExist == false) return false;

            //軟刪除+紀錄編輯時間
            CategoryEntity.IsExist = false;
            CategoryEntity.LastEditTime = DateTime.UtcNow;

            //儲存修改
            await _context.SaveChangesAsync();
            return true;
        }

    }
}
