using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using letterstocrushes.Core.Services;
using letterstocrushes.Core.Model;

namespace letterstocrushes.Controllers
{
    public class BlockController : Controller
    {

        private readonly BlockService _blockService;
        private readonly MailService _mailService;

        public BlockController(BlockService blockService, MailService mailService)
        {
            _blockService = blockService;
            _mailService = mailService;
        }

        public BlockController() : this(new Core.Services.BlockService(new Infrastructure.Data.EfQueryBlocks()),
            new Core.Services.MailService(System.Web.Configuration.WebConfigurationManager.AppSettings["MailPassword"]))
        {
        }

        public ActionResult Index()
        {
            if (User.IsInRole("mod") == false)
            {
                return RedirectToAction("Index", "Home");
            }

            ViewBag.commentsIPban = _blockService.getBlocks(Core.Model.blockType.blockIP, Core.Model.blockWhat.blockComment);
            ViewBag.commentsIPsubnetban = _blockService.getBlocks(Core.Model.blockType.blockSubnet, Core.Model.blockWhat.blockComment);
            ViewBag.IPsubnetban = _blockService.getBlocks(Core.Model.blockType.blockSubnet, Core.Model.blockWhat.blockLetter);
            ViewBag.IPban = _blockService.getBlocks(Core.Model.blockType.blockIP, Core.Model.blockWhat.blockLetter);
            ViewBag.chatIPban = _blockService.getBlocks(Core.Model.blockType.blockIP, Core.Model.blockWhat.blockChat);
            ViewBag.chatIPsubnetban = _blockService.getBlocks(Core.Model.blockType.blockSubnet, Core.Model.blockWhat.blockChat);

            return View();
        }

        [HttpPost]
        public ActionResult Index(FormCollection fc)
        {

            if (User.IsInRole("mod") == false)
            {
                return RedirectToAction("Index", "Home");
            }

            String who = fc["who"].ToString();
            int what = int.Parse(fc["what"]);
            int style = int.Parse(fc["style"]);
            string notes = fc["notes"].ToString();

            _blockService.Add(style, what, who, notes);

            // send a message

            String contact_message = User.Identity.Name + " added a block: <br /><br />" + notes;

            string what_nice = "";
            switch (what)
            {
                case (int)blockWhat.blockLetter:
                    what_nice = "letter";
                    break;
                case (int)blockWhat.blockComment:
                    what_nice = "comment";
                    break;
                case (int)blockWhat.blockChat:
                    what_nice = "chat";
                    break;
            }

            string style_nice = "";
            switch (style)
            {
                case (int)blockType.blockIP:
                    style_nice = "ip";
                    break;
                case (int)blockType.blockSubnet:
                    style_nice = "subnet";
                    break;
                case (int)blockType.blockGUID:
                    style_nice = "guid";
                    break;
            }

            contact_message = contact_message + "what: " + what_nice + "<br />";
            contact_message = contact_message + "style: " + style_nice + "<br />";
            contact_message = contact_message + "who: " + who;

            _mailService.SendContact(contact_message, "seth.hayward@gmail.com");

            return RedirectToAction("Index");
        }


        [HttpGet]
        public ActionResult Remove(int id = 0)
        {

            Block removed = _blockService.getBlock(id);
            _blockService.Remove(id);

            // send a message
            String contact_message = User.Identity.Name + " removed a block: <br /><br />";

            string what_nice = "";
            switch (removed.What)
            {
                case (int)blockWhat.blockLetter:
                    what_nice = "letter";
                    break;
                case (int)blockWhat.blockComment:
                    what_nice = "comment";
                    break;
                case (int)blockWhat.blockChat:
                    what_nice = "chat";
                    break;
            }

            string style_nice = "";
            switch (removed.Type)
            {
                case (int)blockType.blockIP:
                    style_nice = "ip";
                    break;
                case (int)blockType.blockSubnet:
                    style_nice = "subnet";
                    break;
                case (int)blockType.blockGUID:
                    style_nice = "guid";
                    break;
            }

            contact_message = contact_message + "what: " + what_nice + "<br />";
            contact_message = contact_message + "style: " + style_nice + "<br />";
            contact_message = contact_message + "who: " + removed.Value;

            _mailService.SendContact(contact_message, "seth.hayward@gmail.com");

            return RedirectToAction("Index");
        }

    }
}
