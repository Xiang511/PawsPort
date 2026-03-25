using Microsoft.AspNetCore.Mvc;
using PawsPort.Models;

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
    }
}
