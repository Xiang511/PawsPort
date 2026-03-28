using Microsoft.AspNetCore.Mvc;
using PawsPort.Models;
using System.Linq;

namespace PawsPort.Controllers
{
    public class LineBotController : Controller
    {
        public ActionResult List()
        {
            PetDbContext db = new PetDbContext();

            var typeOrder = new List<string>
            {
                    "認養",
                    "醫療",
                    "帳號",
                    "系統",
                    "遊戲",
                    "其他"
            };

            var listFromDb = db.LineBots.ToList();

            var messages = listFromDb
        .OrderBy(b =>
        {

            int index = typeOrder.IndexOf(b.QuestionType);

            return index == -1 ? 99 : index;
        })
        .ThenByDescending(b => b.ChatDate)
        .ToList();


            return View(messages);
        }


        public ActionResult Reply(int? id)
        {
            if (id == null) return RedirectToAction("List");

            PetDbContext db = new PetDbContext();

            var message = db.LineBots.FirstOrDefault(b => b.Id == id);

            if (message == null) return RedirectToAction("List");

            return View(message);
        }


        [HttpPost]
        public ActionResult Reply(int Id, string ReplyText)
        {
            // 【未來擴充區】
            // 因為目前資料庫 LineBot 表格沒有 ReplyContent 欄位
            // 要串接 LINE Messaging API，就是寫在這裡！
            // 例如： LineApi.PushMessage(UserId, ReplyText);

            //先寫假回覆待串api
            TempData["SuccessMessage"] = "✅ 成功！回覆訊息已透過 LINE Bot 傳送給該名使用者。";


            return RedirectToAction("List");
        }
    }
}

