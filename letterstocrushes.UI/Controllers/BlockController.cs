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

        public BlockController(BlockService blockService)
        {
            _blockService = blockService;
        }

        public BlockController() : this(new Core.Services.BlockService(new Infrastructure.Data.EfQueryBlocks()))
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

            String who = fc.GetValue("who").RawValue.ToString();
            String what = fc.GetValue("what").RawValue.ToString();
            String style = fc.GetValue("style").RawValue.ToString();

            System.Diagnostics.Debug.Print("who - " + who + ", what - " + what + ", style - " + style);
            return RedirectToAction("Index");
        }

    }
}
