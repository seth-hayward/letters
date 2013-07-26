using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using letterstocrushes.Core.Services;

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

            ViewBag.IPban = _blockService.getBlocks(Core.Model.blockType.blockIP, Core.Model.blockWhat.blockLetter).Count;

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

            _blockService.Add(style, what, who);

            return RedirectToAction("Index");
        }

    }
}
