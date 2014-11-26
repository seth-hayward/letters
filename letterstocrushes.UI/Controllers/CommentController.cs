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
        private readonly MailService _mailService;

        public CommentController(CommentService commentService, MailService mailService)
        {
            _commentService = commentService;
            _mailService = mailService;
        }

        public CommentController() : this(new CommentService(new Infrastructure.Data.EfQueryLetters(), new Infrastructure.Data.EfQueryComments(), new MailService(System.Web.Configuration.WebConfigurationManager.AppSettings["MailPassword"]), new Core.Services.BlockService(new Infrastructure.Data.EfQueryBlocks())),
            new Core.Services.MailService(System.Web.Configuration.WebConfigurationManager.AppSettings["MailPassword"]))
        {
        }

        public ActionResult Index(int page = 1)
        {
            if (User.IsInRole("Mod") == false)
            {
                return RedirectToAction("Login", "Account");
            }
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

        public JsonResult Unhide(int id = 0)
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

            Boolean edit_successful = _commentService.unhideComment(id, commenter_guid, user_name, is_user_mod);

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

            string basic_text = commentText;

            // first, we make sure that the first line
            // is a paragraph
            basic_text = "<p>" + basic_text;

            // then we make sure the last line closes it
            basic_text = basic_text + "</p>";

            // now all line breaks in the middle should
            // start new paragraphs
            basic_text = basic_text.Replace("\n", "</p><p>");

            Boolean edit_successful = _commentService.EditComment(basic_text, id, commenter_guid, user_name, is_user_mod);

            if (edit_successful == true)
            {
                return basic_text;
            }
            else
            {
                return "Unable to edit comment. Please refresh and try again.";
            }

        }

        [HttpGet]
        [ValidateInput(false)]
        public String CommentText(int id = 0)
        {

            if (id == 0)
            {
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

            Comment c = _commentService.getComment(id);

            // remove html...

            c.commentMessage = c.commentMessage.Replace("</p>", "\n\n");
            c.commentMessage = c.commentMessage.Replace("<p>", "");

            // remove trailing line breaks now...
            c.commentMessage = c.commentMessage.TrimEnd();

            if (c.commentMessage.Length > 0)
            {
                return c.commentMessage;
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

        [HttpGet]
        public JsonResult GetRecentComments(int page)
        {

            if (User.IsInRole("Mod") == false)
            {
                return Json("Oh.", JsonRequestBehavior.AllowGet);
            }

            return Json(_commentService.getRecentComments(page), JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public JsonResult GetComments(int id)
        {
            List<Core.Model.Comment> comments = new List<Core.Model.Comment>();

            comments = _commentService.getComments(id, false);

            // hide some info
            foreach (Comment c in comments)
            {
                c.commenterGuid = "";
                c.commenterIP = "";
                c.commenterEmail = "";
            }

            return Json(comments, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        [ValidateInput(false)]
        public JsonResult Add(int letterId, string comment, string commenterName, string commenterEmail)
        {

            Comment comm = new Comment();
            comm.commentDate = DateTime.UtcNow;
            comm.commenterEmail = commenterEmail;
            comm.commenterName = commenterName;
            comm.commentMessage = comment;
            comm.letterId = letterId;
            comm.level = 0;

            string userip = string.Empty;
            userip = Request.ServerVariables["HTTP_X_FORWARDED_FOR"];
            if (userip == null || userip == "127.0.0.1")
                userip = Request.ServerVariables["REMOTE_ADDR"];
            comm.commenterIP = userip;

            string current_guid = getCommenterGuid();
            if (current_guid == null)
            {
                current_guid = System.Guid.NewGuid().ToString();

                // new commenter, let's give them a guid
                HttpCookie cookie = new HttpCookie("cId");
                cookie.Value = current_guid;
                cookie.Expires = DateTime.Now.AddDays(1500);
                Response.Cookies.Add(cookie);
            }

            comm.commenterGuid = current_guid;

            // add comment, 
            // notify all users who are subscribed to that letter
            string host = "";

            switch (Request.Url.Port)
            {
                case 80:
                    host = "http://" + Request.Url.Host + VirtualPathUtility.ToAbsolute("~/");
                    break;
                default:
                    host = "http://" + Request.Url.Host + ":" + Request.Url.Port + VirtualPathUtility.ToAbsolute("~/");
                    break;
            }

            _commentService.AddComment(comm, host);

            return Json(comm, JsonRequestBehavior.AllowGet);
        }

    }
}
