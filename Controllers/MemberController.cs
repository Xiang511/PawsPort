using Microsoft.AspNetCore.Mvc;
using PawsPort.Models;
using PawsPort.ViewModels;

namespace PawsPort.Controllers
{
    public class MemberController : Controller
    {
        public IActionResult List()
        {
            PetDbContext db = new PetDbContext();
            var p = db.UserTables.ToList();
            return View(p);
        }

        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        public IActionResult Create(UserTable user)
        {   
            PetDbContext db = new PetDbContext();
            db.UserTables.Add(user);
            db.SaveChanges();
            return RedirectToAction("List");
        }


        public IActionResult Edit(int? id)
        {
            if (id == null)
                return RedirectToAction("List");

            PetDbContext db = new PetDbContext();
            UserTable x = db.UserTables.Where(m => m.UserId == id).FirstOrDefault();

            if (x == null)
                return RedirectToAction("List");

            return View(x);
        }

        [HttpPost]
        public IActionResult Edit(UserTable uiUser)
        {
            PetDbContext db = new PetDbContext();
            UserTable DbUser = db.UserTables.Where(m => m.UserId == uiUser.UserId).FirstOrDefault();

            if (DbUser != null)
            {
                DbUser.Name = uiUser.Name;
                DbUser.Photo = uiUser.Photo;
                DbUser.Job = uiUser.Job;
                DbUser.Phone = uiUser.Phone;
                DbUser.City = uiUser.City;
                DbUser.Note = uiUser.Note;
                DbUser.Birthday = uiUser.Birthday;
                DbUser.Status = uiUser.Status;
                DbUser.HasPriorExp = uiUser.HasPriorExp;
                DbUser.Point = uiUser.Point;
                DbUser.IsSubscribe = uiUser.IsSubscribe;
                DbUser.IsVerify = uiUser.IsVerify;
                DbUser.UpdatedAt = DateTime.Now;

                db.SaveChanges();
            }

            return RedirectToAction("List");
        }


        public IActionResult Delete(int? id)
        {
            if (id == null)
                return RedirectToAction("List");

            PetDbContext db = new PetDbContext();
            UserTable x = db.UserTables.Where(m => m.UserId == id).FirstOrDefault();

            if (x != null)
            {
                db.UserTables.Remove(x);
                db.SaveChanges();
            }

            return RedirectToAction("List");
        }
    }
}
