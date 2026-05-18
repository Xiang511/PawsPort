using Microsoft.EntityFrameworkCore;
using PawsPort.Dtos;
using PawsPort.Models;
using Serilog;

namespace PawsPort.Services
{
    public class QuestionsService
    {
        private readonly PetDbContext _db;

        public QuestionsService(PetDbContext db)
        {
            _db = db;
        }

        // 取得題庫列表 (Async 版)
        public async Task<List<QuestionsListDTO>> GetQuestionsListAsync(string GameCategory)
        {
            var query = _db.GameContents.AsQueryable();

            if (!string.IsNullOrEmpty(GameCategory))
            {
                query = query.Where(g => g.GameName == GameCategory);
            }

            return await query.Select(g => new QuestionsListDTO
            {
                GameId = g.GameId,
                GameName = g.GameName,
                Questions = g.Questions,
                AnswersDetail = g.AnswersDetail,
                Answers = g.Answers,
                IsActive = g.IsActive,
                Rewards = g.Rewards,
                Type = g.Type
            }).ToListAsync();
        }

        // 取得分類列表 (Async 版)
        public async Task<List<string>> GetCategoriesAsync()
        {
            return await _db.GameContents
                .Select(g => g.GameName)
                .Distinct()
                .OrderBy(g => g)
                .ToListAsync();
        }

        // 新增題目 (Async 版)
        public async Task CreateQuestionAsync(QuestionsCreateDTO QuestionsCreateDto)
        {
            var newGame = new GameContent
            {
                GameName = QuestionsCreateDto.GameName,
                Questions = QuestionsCreateDto.Questions,
                AnswersDetail = QuestionsCreateDto.AnswersDetail,
                Answers = QuestionsCreateDto.Answers,
                IsActive = QuestionsCreateDto.IsActive,
                Rewards = QuestionsCreateDto.Rewards,
                Type = QuestionsCreateDto.Type
            };

            _db.GameContents.Add(newGame);
            await _db.SaveChangesAsync();
            Log.Information("已新增題目: {GameName}", QuestionsCreateDto.GameName);
        }

        // 更新題目 (Async 版)
        public async Task UpdateQuestionAsync(QuestionsEditDTO QuestionsEditDto)
        {
            // 使用 FirstOrDefaultAsync()
            var game = await _db.GameContents.FirstOrDefaultAsync(g => g.GameId == QuestionsEditDto.GameId);

            if (game == null)
                throw new Exception("題目不存在");

            Log.Information("找到題目 ID: {GameId}, 準備更新", QuestionsEditDto.GameId);
            game.GameName = QuestionsEditDto.GameName;
            game.Questions = QuestionsEditDto.Questions;
            game.AnswersDetail = QuestionsEditDto.AnswersDetail;
            game.Answers = QuestionsEditDto.Answers;
            game.IsActive = QuestionsEditDto.IsActive;
            game.Rewards = QuestionsEditDto.Rewards;
            game.Type = QuestionsEditDto.Type;

            await _db.SaveChangesAsync(); 
            Log.Information("題目 {GameId} 更新完成", QuestionsEditDto.GameId);
        }

        // 刪除題目 (Async 版)
        public async Task DeleteQuestionAsync(int id)
        {
            // 使用 FirstOrDefaultAsync()
            var game = await _db.GameContents.FirstOrDefaultAsync(g => g.GameId == id);

            if (game == null)
                throw new Exception("題目不存在");

            _db.GameContents.Remove(game);
            await _db.SaveChangesAsync();
            Log.Information("題目 ID: {GameId} 已刪除", id);
        }

        
        public async Task<List<QuestionsListDTO>> GetLevelQuestionsAsync(string category, int limit = 10)
        {
            // 1. 核心過濾：必須符合傳入的分類(GameName)、且啟用(IsActive)、且 Type 必須是 "問答"
            var query = _db.GameContents
                .Where(g => g.GameName == category
                         && g.IsActive == true
                         && g.Type == "問答"); // 嚴格限制只要問答題

            // 2. 透過 Guid 隨機排序，並限制只取 10 題
            return await query
                .OrderBy(g => Guid.NewGuid())
                .Take(limit)
                .Select(g => new QuestionsListDTO
                {
                    GameId = g.GameId,
                    GameName = g.GameName,
                    Questions = g.Questions,       // 讀取題目內容
                    Answers = g.Answers,           // 答案
                    AnswersDetail = g.AnswersDetail, // 詳細解答
                    Rewards = g.Rewards,           // 答題獎勵
                    IsActive = g.IsActive,
                    Type = g.Type
                })
                .ToListAsync();
        }
    }



}