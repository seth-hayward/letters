using System;
using System.Collections.Generic;
using System.Linq;
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
            List<Core.Model.Chat> chats = _chatService.PopulateChatMessagesFromDatabase();
            ViewData.Model = chats;
            return View();
        }

    }
}
