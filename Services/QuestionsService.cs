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

        // 取得題庫列表
        public List<QuestionsListDTO> GetQuestionsList(string GameCategory)
        {
            var query = _db.GameContents.AsQueryable();

            if (!string.IsNullOrEmpty(GameCategory))
            {
                query = query.Where(g => g.GameName == GameCategory);
            }

            return query.Select(g => new QuestionsListDTO
            {
                GameId = g.GameId,
                GameName = g.GameName,
                Questions = g.Questions,
                AnswersDetail = g.AnswersDetail,
                Answers = g.Answers,
                IsActive = g.IsActive,
                Rewards = g.Rewards,
                Type = g.Type
            }).ToList();
        }

        // 取得分類列表
        public List<string> GetCategories()
        {
            return _db.GameContents
                .Select(g => g.GameName)
                .Distinct()
                .OrderBy(g => g)
                .ToList();
        }

        // 新增題目
        public void CreateQuestion(QuestionsCreateDTO QuestionsCreateDto)
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
            _db.SaveChanges();
            Log.Information("已新增題目: {GameName}", QuestionsCreateDto.GameName);
        }

        // 更新題目
        public void UpdateQuestion(QuestionsEditDTO QuestionsEditDto)
        {
            var game = _db.GameContents.FirstOrDefault(g => g.GameId == QuestionsEditDto.GameId);
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

            _db.SaveChanges();
            Log.Information("題目 {GameId} 更新完成", QuestionsEditDto.GameId);
        }

        // 刪除題目
        public void DeleteQuestion(int id)
        {
            var game = _db.GameContents.FirstOrDefault(g => g.GameId == id);
            if (game == null)
                throw new Exception("題目不存在");

            _db.GameContents.Remove(game);
            _db.SaveChanges();
            Log.Information("題目 ID: {GameId} 已刪除", id);
        }
    }
}