using PawsPort.Dtos;
using PawsPort.Models;
using Microsoft.EntityFrameworkCore;


namespace PawsPort.Services
{
    public class CommentService
    {
        private readonly PetDbContext _context;

        public CommentService(PetDbContext context)
        {
            _context = context;
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
    }
}
