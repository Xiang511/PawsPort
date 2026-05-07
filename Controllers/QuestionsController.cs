using Microsoft.AspNetCore.Mvc;
using PawsPort.Dtos;
using PawsPort.Models;
using PawsPort.Services;
using Serilog;

namespace PawsPort.Controllers
{
    [Route("api/[controller]")]
    public class QuestionsController : ApiControllerBase
    {
        private readonly QuestionsService _questionsService;

        public QuestionsController(QuestionsService questionsService)
        {
            _questionsService = questionsService;
        }

        // GET: api/Questions
        [HttpGet]
        public async Task<IActionResult> List(string category = "")
        {
            try
            {
                var data = await _questionsService.GetQuestionsListAsync(category);
                var categories = await _questionsService.GetCategoriesAsync();

                return Success(new
                {
                    QuestionContent = data,
                    Categories = categories,
                    SelectedCategory = category
                }, "取得成功", 200);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "QuestionsController: 取得題庫列表失敗");
                return Failure("QUESTIONS_LIST_FAILED", "伺服器取得資料失敗", 500);
            }
        }

        // POST: api/Questions
        [HttpPost]
        public async Task<IActionResult> Create(QuestionsCreateDTO createDto)
        {
            try
            {
                Log.Information("GameName: {GameName}", createDto.GameName);
                Log.Information("Questions: {Questions}", createDto.Questions);
                Log.Information("AnswersDetail: {AnswersDetail}", createDto.AnswersDetail);
                Log.Information("Answers: {Answers}", createDto.Answers);
                Log.Information("IsActive: {IsActive}", createDto.IsActive);
                Log.Information("Rewards: {Rewards}", createDto.Rewards);
                Log.Information("Type: {Type}", createDto.Type);
                await _questionsService.CreateQuestionAsync(createDto);
                return Success(createDto, "新增成功", 200);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "QuestionsController: 新增失敗");
                return Failure("QUESTION_CREATE_FAILED", "儲存題目時發生錯誤", 500);
            }
        }

        // PUT: api/Questions/{id}
        [HttpPut("{id}")]
        public async Task<IActionResult> Edit(int id, QuestionsEditDTO editDto)
        {
            if (id != editDto.GameId)
                return Failure("QUESTION_ID_MISMATCH", "網址 ID 與資料內容不符", 400);

            try
            {
                await _questionsService.UpdateQuestionAsync(editDto);
                return Success(editDto, "更新成功", 200);
            }
            catch (Exception ex)
            {
                if (ex.Message.Contains("找不到"))
                    return Failure("QUESTION_NOT_FOUND", "找不到該題目", 404);

                Log.Error(ex, "QuestionsController: 更新 ID {id} 失敗", id);
                return Failure("QUESTION_UPDATE_FAILED", "更新過程發生錯誤", 500);
            }
        }

        // DELETE: api/Questions/{id}
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                await _questionsService.DeleteQuestionAsync(id);
                return Success(id, "刪除成功", 200);
            }
            catch (Exception ex)
            {
                if (ex.Message.Contains("找不到"))
                    return Failure("QUESTION_NOT_FOUND", "找不到欲刪除的題目", 404);

                Log.Error(ex, "QuestionsController: 刪除 ID {id} 失敗", id);
                return Failure("QUESTION_DELETE_FAILED", "刪除過程發生錯誤", 500);
            }
        }
    }
}