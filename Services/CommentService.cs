using PawsPort.Dtos;
using PawsPort.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Identity.Client;


namespace PawsPort.Services
{
    public class CommentService
    {
        private readonly PetDbContext _context;

        public CommentService(PetDbContext context)
        {
            _context = context;
        }

        
        //取得某篇文章的留言列表
        public async Task<List<CommentListDTO>> GetCommentsByArticleIdAsync(int articleId)
        {
            var comments = await (
                from c in _context.Comments
                join u in _context.UserTables
                    on c.UserId equals u.UserId
                where c.ArticleId == articleId
                      && c.IsExist == true
                      && c.IsActive == true
                       && c.Status == 1
                orderby c.CreateAt ascending
                select new CommentListDTO
                {
                    CommentId = c.CommentId,
                    ArticleId = c.ArticleId,
                    UserId = c.UserId,
                    UserName = u.Name,
                    UserPhoto = u.Photo,
                    Content = c.Content,
                    CreateAt = c.CreateAt,
                    LastEditTime = c.LastEditTime,
                    ImageUrl = c.ImageUrl,
                    ParentId = c.ParentId
                }
            ).ToListAsync();

            return comments;
        }

        //新增留言
        public async Task<int> CreateCommentAsync(CommentSaveDTO commentSaveDTO)
        {
            //先看看是不是留言樓的留言
            if (commentSaveDTO.ParentId.HasValue)
            {
                //檢查該留言樓是否存在
                var ParentComment = await _context.Comments
                    .AsNoTracking()
                    .FirstOrDefaultAsync(c => c.CommentId == commentSaveDTO.ParentId && c.IsExist == true && c.IsActive == true);

                if (ParentComment == null) throw new Exception("找不到指定的留言樓");
                if (ParentComment.ParentId.HasValue) throw new Exception("目前僅支援兩層留言結構，無法回覆此留言");
            }
            //檢查通過，新增留言
            var CommentEntity = new Comment
            {
                UserId = commentSaveDTO.UserId,
                ArticleId = commentSaveDTO.ArticleId,
                ParentId = commentSaveDTO.ParentId,
                Content = commentSaveDTO.Content,
                ImageUrl = commentSaveDTO.ImageUrl,
                Status = commentSaveDTO.Status,

                CreateAt = DateTime.UtcNow,
                LastEditTime = DateTime.UtcNow,
                IsExist = true,
                IsActive = true,

                ReportedCount = 0,
                LastReported = null,
                DeleteTypeId = null,
                DeleteNote = null
            };
            //儲存
            _context.Comments.Add(CommentEntity);
            await _context.SaveChangesAsync();
            //返回留言id
            return CommentEntity.CommentId;

        }

        //刪除留言
        public async Task<bool> DeleteCommentAsync(int id)
        {
            //撈看看有無此留言
            var CommentEntity = await _context.Comments.FirstOrDefaultAsync(c => c.CommentId == id);

            if(CommentEntity==null||CommentEntity.IsExist==false) return false;

            if(CommentEntity.IsActive == false) throw new InvalidOperationException("該留言因違反規範已被屏蔽，無法執行刪除。");

            CommentEntity.IsExist = false;
            CommentEntity.LastEditTime = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            return true;

        }

    }
}
