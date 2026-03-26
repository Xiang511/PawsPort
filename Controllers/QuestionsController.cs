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

            var count = db.GameContents.Count();
            G.GameId = count + 1;
            Debug.WriteLine($"Create GameContent id : {G.GameId}");

            db.GameContents.Add(G);
            db.SaveChanges();
            return RedirectToAction("List");
        }
 
        public IActionResult Create2()
        {
            return View();

        }

        [HttpPost]
        public IActionResult Create2(GameContent G)
        {
            PetDbContext db = new PetDbContext();
            db.GameContents.Add(G);
            db.SaveChanges();
            return RedirectToAction("List");
        }


    }
}
