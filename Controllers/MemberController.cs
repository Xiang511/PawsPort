using Humanizer;
using Microsoft.AspNetCore.Mvc;
using PawsPort.Models;
using PawsPort.ViewModels;
using System.Diagnostics;

namespace PawsPort.Controllers
{
    public class MemberController : Controller
    {

        private readonly PetDbContext _context;
        public MemberController(PetDbContext context)
        {
            _context = context;
        }
        public IActionResult List()
        {
            var MemberCount = _context.UserTables.Where(x=>x.DeleteDay == null).Count();
            ViewBag.MemberCount = MemberCount;

            DateTime today = DateTime.Now;     
            DateTime startOfMonth = new DateTime(today.Year, today.Month, 1);   // 2. 取得這個月的第一天 (時分秒自動為 00:00:00)      
            DateTime startOfNextMonth = startOfMonth.AddMonths(1);  // 3. 取得下個月的第一天   
            var MemberMonthSignUp = _context.UserTables      // 4. 進行範圍查詢 (效能最佳！)
                .Count(u => u.CreatedAt >= startOfMonth && u.CreatedAt < startOfNextMonth);
            ViewBag.MemberMonthSignUp = MemberMonthSignUp;

            var MemberVerify = _context.UserTables.Where(x => x.IsVerify == true && x.DeleteDay == null).Count();
            float Verify = ((float)MemberVerify / MemberCount)*100;
            string displayVerify = Verify.ToString("F1");
            Debug.WriteLine(displayVerify);
            ViewBag.Verify = displayVerify;

            var MemberRss = _context.UserTables.Where(x => x.IsSubscribe == true && x.DeleteDay == null).Count();
            ViewBag.MemberRss = MemberRss;

            var p = _context.UserTables.Where(x=>x.DeleteDay == null).ToList();
            return View(p);
        }

        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        public IActionResult Create(UserTable user)
        {   
            user.CreatedAt = DateTime.Now;
            user.UpdatedAt = DateTime.Now;
            _context.UserTables.Add(user);
            _context.SaveChanges();
            return RedirectToAction("List");
        }


        public IActionResult Edit(int? id)
        {
            if (id == null)
                return RedirectToAction("List");

            UserTable x = _context.UserTables.Where(m => m.UserId == id).FirstOrDefault();

            if (x == null)
                return RedirectToAction("List");

            return View(x);
        }

        [HttpPost]
        public IActionResult Edit(UserTable uiUser)
        {
            UserTable DbUser = _context.UserTables.Where(m => m.UserId == uiUser.UserId).FirstOrDefault();

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

                _context.SaveChanges();
            }

            return RedirectToAction("List");
        }


        public IActionResult Delete(int? id)
        {
            if (id == null)
                return RedirectToAction("List");

            UserTable x = _context.UserTables.Where(m => m.UserId == id).FirstOrDefault();

            if (x != null)
            {
                x.DeleteDay = DateTime.Now;
                x.UpdatedAt = DateTime.Now;
                _context.SaveChanges();
            }

            return RedirectToAction("List");
        }
    }
}
