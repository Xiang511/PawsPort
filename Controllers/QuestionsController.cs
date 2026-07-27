using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PawsPort.Dtos;
using PawsPort.Models;
using PawsPort.Services;
using Serilog;

namespace PawsPort.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Produces("application/json")]
    [Tags("遊戲系統")]
    public class QuestionsController : ApiControllerBase
    {
        private readonly QuestionsService _questionsService;

        public QuestionsController(QuestionsService questionsService)
        {
            _questionsService = questionsService;
        }

        // GET: api/Questions
        /// <summary>
        /// 取得題庫列表
        /// </summary>
        /// <param name="category">題目分類（選填）</param>
        /// <returns>包含題目內容與分類選項的 JSON</returns>
        /// <response code="200">成功取得題庫列表</response>
        [Authorize(Policy = "遊戲系統_普通管理員")]
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
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
        /// <summary>
        /// 創建新題目
        /// </summary>
        /// <param name="createDto">題目建立資料</param>
        /// <returns>建立成功的題目資料</returns>
        /// <response code="200">成功新增題目</response>
        [Authorize(Policy = "遊戲系統_普通管理員")]
        [HttpPost]
        [ProducesResponseType(typeof(QuestionsCreateDTO), StatusCodes.Status200OK)]
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
        /// <summary>
        /// 更新指定題目資訊
        /// </summary>
        /// <param name="id">題目 ID</param>
        /// <param name="editDto">更新的題目資料</param>
        /// <returns>更新後的資料</returns>
        /// <response code="200">成功更新題目</response>
        /// <response code="400">ID 不一致</response>
        /// <response code="404">找不到該題目</response>
        [Authorize(Policy = "遊戲系統_普通管理員")]
        [HttpPut("{id}")]
        [ProducesResponseType(typeof(QuestionsEditDTO), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
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
        /// <summary>
        /// 刪除指定題目
        /// </summary>
        /// <param name="id">題目 ID</param>
        /// <response code="200">成功刪除題目</response>
        /// <response code="404">找不到欲刪除的題目</response>
        [Authorize(Policy = "遊戲系統_普通管理員")]
        [HttpDelete("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
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

        // GET: api/Questions/game-level?category=認養須知
        /// <summary>
        /// 遊戲前台：根據關卡分類取得題目(隨機)
        /// </summary>
        /// <param name="category">關卡分類名稱 (GameName，例如：認養須知)</param>
        [Authorize(Policy = "遊戲系統_一般成員")]
        [HttpGet("game-level")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> GetGameLevelQuestions([FromQuery] string category)
        {
            // 商業邏輯驗證：如果前端漏傳參數，主動回傳 Failure（這不是系統崩潰，是屬於正常驗證，所以不用 try/catch）
            if (string.IsNullOrEmpty(category))
            {
                return Failure("CATEGORY_REQUIRED", "必須提供關卡分類名稱（GameName）", 400);
            }

            // 核心業務：直接呼叫 Service 撈取資料。
            var questions = await _questionsService.GetLevelQuestionsAsync(category, 10);

            // 傳回成功包裝的 JSON
            return Success(questions, "成功取得遊戲關卡題庫", 200);
        }
    }
}
