using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using Microsoft.AspNet.SignalR.Client.Hubs;

namespace letterstocrushes.Controllers
{
    public class ChatController : Controller
    {

        private readonly Core.Services.ChatService _chatService;

        public ChatController(Core.Services.ChatService chatService)
        {
            _chatService = chatService;
        }

        public ChatController() : this(new Core.Services.ChatService(new Infrastructure.Data.EfQueryChats()))
        {
        }

        // disable caching on index page
        [OutputCacheAttribute(VaryByParam = "*", Duration = 0, NoStore = true)]
        public ActionResult Index()
        {            
            return View();
        }

        public ActionResult Active()
        {
            return View();
        }

        public ActionResult History()
        {

            List<Core.Model.Chat> chats = _chatService.PopulateChatMessagesFromDatabase("1");

            if (HttpContext.Cache["startChatDate"] != null)
            {
                DateTime startChatDate = (DateTime)HttpContext.Cache["startChatDate"];
                chats = (from m in chats where m.ChatDate > startChatDate select m).ToList();
            }

            ViewData.Model = chats;
            return View();
        }

    }
}
