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
    [Tags("¹CÀ¸¨t²Î")]
    public class QuestionsController : ApiControllerBase
    {
        private readonly QuestionsService _questionsService;

        public QuestionsController(QuestionsService questionsService)
        {
            _questionsService = questionsService;
        }

        // GET: api/Questions
        /// <summary>
        /// ¨ú±oÃD®w¦Cªí
        /// </summary>
        /// <param name="category">ÃD¥Ø¤ÀÃş¡]¿ï¶ñ¡^</param>
        /// <returns>¥]§tÃD¥Ø¤º®e»P¤ÀÃş¿ï¶µªº JSON</returns>
        /// <response code="200">¦¨¥\¨ú±oÃD®w¦Cªí</response>
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
                }, "¨ú±o¦¨¥\", 200);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "QuestionsController: ¨ú±oÃD®w¦Cªí¥¢±Ñ");
                return Failure("QUESTIONS_LIST_FAILED", "¦øªA¾¹¨ú±o¸ê®Æ¥¢±Ñ", 500);
            }
        }

        // POST: api/Questions
        /// <summary>
        /// ³Ğ«Ø·sÃD¥Ø
        /// </summary>
        /// <param name="createDto">ÃD¥Ø«Ø¥ß¸ê®Æ</param>
        /// <returns>«Ø¥ß¦¨¥\ªºÃD¥Ø¸ê®Æ</returns>
        /// <response code="200">¦¨¥\·s¼WÃD¥Ø</response>
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
                return Success(createDto, "·s¼W¦¨¥\", 200);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "QuestionsController: ·s¼W¥¢±Ñ");
                return Failure("QUESTION_CREATE_FAILED", "Àx¦sÃD¥Ø®Éµo¥Í¿ù»~", 500);
            }
        }

        // PUT: api/Questions/{id}
        /// <summary>
        /// §ó·s«ü©wÃD¥Ø¸ê°T
        /// </summary>
        /// <param name="id">ÃD¥Ø ID</param>
        /// <param name="editDto">§ó·sªºÃD¥Ø¸ê®Æ</param>
        /// <returns>§ó·s«áªº¸ê®Æ</returns>
        /// <response code="200">¦¨¥\§ó·sÃD¥Ø</response>
        /// <response code="400">ID ¤£¤@­P</response>
        /// <response code="404">§ä¤£¨ì¸ÓÃD¥Ø</response>
        [HttpPut("{id}")]
        [ProducesResponseType(typeof(QuestionsEditDTO), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Edit(int id, QuestionsEditDTO editDto)
        {
            if (id != editDto.GameId)
                return Failure("QUESTION_ID_MISMATCH", "ºô§} ID »P¸ê®Æ¤º®e¤£²Å", 400);

            try
            {
                await _questionsService.UpdateQuestionAsync(editDto);
                return Success(editDto, "§ó·s¦¨¥\", 200);
            }
            catch (Exception ex)
            {
                if (ex.Message.Contains("§ä¤£¨ì"))
                    return Failure("QUESTION_NOT_FOUND", "§ä¤£¨ì¸ÓÃD¥Ø", 404);

                Log.Error(ex, "QuestionsController: §ó·s ID {id} ¥¢±Ñ", id);
                return Failure("QUESTION_UPDATE_FAILED", "§ó·s¹Lµ{µo¥Í¿ù»~", 500);
            }
        }

        // DELETE: api/Questions/{id}
        /// <summary>
        /// §R°£«ü©wÃD¥Ø
        /// </summary>
        /// <param name="id">ÃD¥Ø ID</param>
        /// <response code="200">¦¨¥\§R°£ÃD¥Ø</response>
        /// <response code="404">§ä¤£¨ì±ı§R°£ªºÃD¥Ø</response>
        [HttpDelete("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                await _questionsService.DeleteQuestionAsync(id);
                return Success(id, "§R°£¦¨¥\", 200);
            }
            catch (Exception ex)
            {
                if (ex.Message.Contains("§ä¤£¨ì"))
                    return Failure("QUESTION_NOT_FOUND", "§ä¤£¨ì±ı§R°£ªºÃD¥Ø", 404);

                Log.Error(ex, "QuestionsController: §R°£ ID {id} ¥¢±Ñ", id);
                return Failure("QUESTION_DELETE_FAILED", "§R°£¹Lµ{µo¥Í¿ù»~", 500);
            }
        }

        // GET: api/Questions/game-level?category=èªé¤Šé ˆçŸ¥
        /// <summary>
        /// éŠæˆ²å‰å°ï¼šæ ¹æ“šé—œå¡åˆ†é¡å–å¾—é¡Œç›®(éš¨æ©Ÿ)
        /// </summary>
        /// <param name="category">é—œå¡åˆ†é¡åç¨± (GameNameï¼Œä¾‹å¦‚ï¼šèªé¤Šé ˆçŸ¥)</param>
        [HttpGet("game-level")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> GetGameLevelQuestions([FromQuery] string category)
        {
            // å•†æ¥­é‚è¼¯é©—è­‰ï¼šå¦‚æœå‰ç«¯æ¼å‚³åƒæ•¸ï¼Œä¸»å‹•å›å‚³ Failureï¼ˆé€™ä¸æ˜¯ç³»çµ±å´©æ½°ï¼Œæ˜¯å±¬æ–¼æ­£å¸¸é©—è­‰ï¼Œæ‰€ä»¥ä¸ç”¨ try/catchï¼‰
            if (string.IsNullOrEmpty(category))
            {
                return Failure("CATEGORY_REQUIRED", "å¿…é ˆæä¾›é—œå¡åˆ†é¡åç¨±ï¼ˆGameNameï¼‰", 400);
            }

            // æ ¸å¿ƒæ¥­å‹™ï¼šç›´æ¥å‘¼å« Service æ’ˆå–è³‡æ–™ã€‚
            var questions = await _questionsService.GetLevelQuestionsAsync(category, 10);

            // å‚³å›æˆåŠŸåŒ…è£çš„ JSON
            return Success(questions, "æˆåŠŸå–å¾—éŠæˆ²é—œå¡é¡Œåº«", 200);
        }
    }
}