﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Microsoft.AspNet.SignalR.Client.Hubs;

namespace letterstocrushes.Controllers
{
    public class ChatController : Controller
    {
        //
        // GET: /Chat/

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
            return View();
        }

    }
}
