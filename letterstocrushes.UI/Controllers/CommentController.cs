using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using letterstocrushes.Core.Model;
using letterstocrushes.Core.Services;

namespace letterstocrushes.Controllers
{
    public class CommentController : Controller
    {

        private readonly CommentService _commentService;

        public CommentController(CommentService commentService)
        {
            _commentService = commentService;
        }

        public CommentController() : this(new CommentService(new Infrastructure.Data.EfQueryLetters(), new Infrastructure.Data.EfQueryComments(), new MailService(System.Web.Configuration.WebConfigurationManager.AppSettings["MailPassword"])))
        {
        }

        public ActionResult Index(int page = 1)
        {
            ViewBag.CurrentPage = page;
            ViewBag.Pages = _commentService.getCommentCount();
            return View(_commentService.getRecentComments(page));
        }

        public ActionResult Details(int id = 0)
        {

            if (id == 0)
            {
                return View("Index");
            }

            return View(_commentService.getComment(id));
        }

        public JsonResult Hide(int id = 0)
        {

            if (id == 0)
            {
                return Json(false, JsonRequestBehavior.AllowGet);
            }

            bool is_user_mod = User.IsInRole("Mod");

            string userip = "anonymous letter sender";
            userip = Request.ServerVariables["HTTP_X_FORWARDED_FOR"];
            if (userip == null)
                userip = Request.ServerVariables["REMOTE_ADDR"];

            string user_name;
            if (User.Identity.IsAuthenticated == true)
            {
                user_name = User.Identity.Name;
            }
            else
            {
                user_name = userip;
            }

            String commenter_guid = getCommenterGuid();

            Boolean edit_successful = _commentService.hideComment(id, commenter_guid, user_name, is_user_mod);

            return Json(edit_successful, JsonRequestBehavior.AllowGet);

        }

        [HttpPost]
        [ValidateInput(false)]
        public String Edit(string commentText, int id = 0)
        {

            if (id == 0) {
                return "Comment not found.";
            }

            bool is_user_mod = User.IsInRole("Mod");

            string userip = "anonymous letter sender";
            userip = Request.ServerVariables["HTTP_X_FORWARDED_FOR"];
            if (userip == null)
                userip = Request.ServerVariables["REMOTE_ADDR"];

            string user_name;
            if (User.Identity.IsAuthenticated == true)
            {
                user_name = User.Identity.Name;
            }
            else
            {
                user_name = userip;
            }

            String commenter_guid = getCommenterGuid();

            Boolean edit_successful = _commentService.EditComment(commentText, id, commenter_guid, user_name, is_user_mod);

            if (edit_successful == true)
            {
                return commentText;
            }
            else
            {
                return "Unable to edit comment. Please refresh and try again.";
            }

        }

        public string getCommenterGuid()
        {
            HttpCookie c_guid = Request.Cookies["cId"];
            string c_guid_value = null;

            if (c_guid == null)
            {
                c_guid_value = null;
            }
            else
            {
                c_guid_value = c_guid.Value;
            }

            return c_guid_value;
        }

    }
}
