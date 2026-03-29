using Microsoft.AspNetCore.Mvc;

namespace PawsPort.Controllers
{
    public class ChatController : Controller
    {
        //public IActionResult Index()
        //{
        //    return View();
        //}


        public IActionResult ChatroomList()
        {

            //要有一個頁面可以搜尋一個會員底下所有的聊天室 =ChatroomList
            //觀察有沒有會員檢舉某些聊天室的訊息，並且查看該訊息是什麼
            //要有顯示區顯示該會員的近期(一周)聊天訊息數量，用來篩選是不是有機器人
            //要有一個顯示區顯示全站近期(一周)的聊天室和聊天訊息數量，用來篩選是不是有機器人


            //要有一個功能可以將該會員的聊天功能暫時停權，並暫時封存所有他底下的聊天室 =ChatroomDelete


            return View();
        }

    }
}
