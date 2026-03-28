using Microsoft.AspNetCore.Mvc;

namespace PawsPort.Controllers
{
    public class ChatController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
