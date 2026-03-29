using Microsoft.AspNetCore.Mvc;
using PawsPort.Models;
using PawsPort.ViewModels;

namespace PawsPort.Controllers
{
    public class ChatController : Controller
    {
        //public IActionResult Index()
        //{
        //    return View();
        //}


        public IActionResult ChatroomList(ChatroomListViewModel vm)
        {

            using (PetDbContext db = new PetDbContext())
            {
                DateTime LimitTime = DateTime.Now.AddDays(-1);  //時間限制在前24小時

                //抓取所有聊天室，並關連訊息進行統計
                var Query = db.Chatrooms.Select(r => new ChatroomItemViewModel
                {
                    ChatroomId = r.ChatroomId,

                    // 直接抓取 User1 和 User2 的名字
                    // 如果 Name 是空的，就抓 Email @ 前面的字 (Split 邏輯) --沒有外鍵太麻煩先不做
                    UserId_1_Name = db.UserTables.Where(u => u.UserId == r.UserId1)
                    .Select(u => u.Name).FirstOrDefault() ?? "未知用戶",

                    UserId_2_Name = db.UserTables.Where(u => u.UserId == r.UserId2)
                    .Select(u => u.Name).FirstOrDefault() ?? "未知用戶",

                    // 抓最後一則訊息
                    LastMsg = db.Messages.Where(m =>m.ChatroomId == r.ChatroomId && m.IsExist) 
                    //指定該聊天室存在的訊息
                    .OrderByDescending(m => m.CreateAt) //以新到舊排序
                    .Select(m => m.Content).FirstOrDefault(),//選取第一個

                    //抓最後傳訊時間
                    LastMsgTime = db.Messages.Where(m => m.ChatroomId == r.ChatroomId && m.IsExist)
                    .OrderByDescending(m => m.CreateAt) //將該聊天室訊息時間由新到舊排
                    .Select(m => (DateTime?)m.CreateAt).FirstOrDefault(), //選取最新的，沒有就傳null

                    // 計算24小時內訊息量
                    MsgCount24h = db.Messages.Count(m =>m.ChatroomId==r.ChatroomId 
                    && m.CreateAt >= LimitTime && m.IsExist)
                });

                // 2. 關鍵字人名搜尋 (針對聊天室名稱)
                if (!string.IsNullOrEmpty(vm.TxtKeyword)) //如果有關鍵字
                {
                    Query = Query.Where(r => r.UserId_1_Name.Contains(vm.TxtKeyword)||r.UserId_2_Name.Contains(vm.TxtKeyword)); //找出名稱含關鍵字的聊天室
                }

                // 3. 執行查詢並排序 (最熱門的排前面)
                if (vm.SortOrder == "desc") //由大到小，由新到舊
                {
                    Query = vm.SortBy switch
                    {
                        // 先按訊息量降序，一樣的話按 ID 降序
                        "MsgCount" => Query.OrderByDescending(r => r.MsgCount24h).ThenByDescending(r => r.ChatroomId),

                        // 先按日期降序，一樣的話按 ID 降序
                        "LastMsg" => Query.OrderByDescending(r => r.LastMsgTime).ThenByDescending(r => r.ChatroomId),

                        "UserName" => Query.OrderByDescending(r => r.UserId_1_Name).ThenByDescending(r => r.ChatroomId),
                        _ => Query.OrderByDescending(r => r.ChatroomId)
                    };
                }
                else
                {
                    Query = vm.SortBy switch
                    {
                        "MsgCount" => Query.OrderBy(r => r.MsgCount24h).ThenBy(r => r.ChatroomId),
                        "LastMsg" => Query.OrderBy(r => r.LastMsgTime).ThenBy(r => r.ChatroomId),
                        "UserName" => Query.OrderBy(r => r.UserId_1_Name).ThenBy(r => r.ChatroomId),
                        _ => Query.OrderBy(r => r.ChatroomId)
                    };
                }

                //分頁
                vm.TotalCount = Query.Count(); //計算總筆數
                //框出當前要看的頁
                vm.ChatroomItems = Query.Skip((vm.CurrentPage - 1) * vm.PageSize) 
                    .Take(vm.PageSize).ToList();
                //跳過((當前頁面-1)*一頁有幾筆).取一頁的筆數.轉換成list


                // 4. 計算上方儀表板總數

                vm.TotalMsg24h =db.Messages.Count(m =>m.CreateAt >= LimitTime && m.IsExist); //計算24小時內幾筆
                vm.ActiveRoomsCount = Query.Count(r => r.MsgCount24h > 0); //全站的活躍中聊天室
                // 機器人數量可以先給假資料，或是額外下查詢算出
                vm.SuspiciousUserCount = 0;

                return View(vm);
            }



            //要有一個頁面可以搜尋一個會員底下所有的聊天室 =ChatroomList
            //觀察有沒有會員檢舉某些聊天室的訊息，並且查看該訊息是什麼
            //要有顯示區顯示該會員的近期(一周)聊天訊息數量，用來篩選是不是有機器人
            //要有一個顯示區顯示全站近期(一周)的聊天室和聊天訊息數量，用來篩選是不是有機器人


            //要有一個功能可以將該會員的聊天功能暫時停權，並暫時封存所有他底下的聊天室 =ChatroomDelete

        
        }

    }
}
