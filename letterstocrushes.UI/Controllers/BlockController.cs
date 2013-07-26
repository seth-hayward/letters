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

        public ActionResult Index()
        {

            ViewBag.IPban = _blockService.getBlocks(Core.Model.blockType.blockIP, Core.Model.blockWhat.blockLetter).Count;

            return View();
        }

    }
}
