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
        public IActionResult List(string category = "")
        {
            var data = _questionsService.GetQuestionsList(category);
            var categories = _questionsService.GetCategories();

            return Success(new
            {
                Data = data,
                Categories = categories,
                SelectedCategory = category
            });
        }

        // POST: api/Questions
        [HttpPost]
        public IActionResult Create(QuestionsCreateDTO createDto)
        {
            Log.Information("GameName: {GameName}", createDto.GameName);
            Log.Information("Questions: {Questions}", createDto.Questions);
            Log.Information("AnswersDetail: {AnswersDetail}", createDto.AnswersDetail);
            Log.Information("Answers: {Answers}", createDto.Answers);
            Log.Information("IsActive: {IsActive}", createDto.IsActive);
            Log.Information("Rewards: {Rewards}", createDto.Rewards);
            Log.Information("Type: {Type}", createDto.Type);
            _questionsService.CreateQuestion(createDto);
            return Success(createDto, "新增成功", 200);
        }

        // PUT: api/Questions/{id}
        [HttpPut("{id}")]
        public IActionResult Edit(QuestionsEditDTO editDto)
        {
            _questionsService.UpdateQuestion(editDto);
            return Success(editDto, "更新成功", 200);
        }

        // DELETE: api/Questions/{id}
        [HttpDelete("{id}")]
        public IActionResult Delete(int id)
        {
            _questionsService.DeleteQuestion(id);
            return Success(id, "刪除成功", 200);
        }
    }
}