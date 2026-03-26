using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PawsPort.Models;
using System.Diagnostics;

namespace PawsPort.Controllers
{
    public class QuestionsController : Controller
    {
        // 題庫列表
        public IActionResult List()
        {
            PetDbContext db = new PetDbContext();
            var GameList = db.GameContents.ToList();
            return View(GameList);
        }
        public IActionResult Create()
        {
            return View();
        }
        [HttpPost]
        public IActionResult Create(GameContent G)
        {
            PetDbContext db = new PetDbContext();
            db.GameContents.Add(G);
            db.SaveChanges();
            return RedirectToAction("List");
        }


        // 編輯題目 - GET 方法
        public IActionResult Edit(int? id)
        {
            PetDbContext db = new PetDbContext();
            GameContent game = db.GameContents.FirstOrDefault(game => game.GameId == id);
            if (game == null)
                return RedirectToAction("List");
            return View(game);
        }

        // 編輯題目 - POST 方法
        [HttpPost]
        public IActionResult Edit(GameContent G)
        {
            PetDbContext db = new PetDbContext();
            GameContent game = db.GameContents.FirstOrDefault(game => game.GameId == G.GameId);
            if (game != null)
            {
                game.GameName = G.GameName;
                game.Questions = G.Questions;
                game.AnswersDetail = G.AnswersDetail;
                game.Answers = G.Answers;
                game.IsActive = G.IsActive;
                game.Rewards = G.Rewards;
                game.Type = G.Type;
                db.SaveChanges();
            }
            return RedirectToAction("List");
        }

    }
}
