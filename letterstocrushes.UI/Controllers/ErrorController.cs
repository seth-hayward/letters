using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Net.Mail;
using System.Diagnostics;

namespace letterstocrushes.Controllers
{
    public class ErrorController : Controller
    {

        private readonly Core.Services.MailService _mailService;
        public ErrorController(Core.Services.MailService mailService)
        {
            _mailService = mailService;
        }

        public ErrorController()
            : this(new Core.Services.MailService(System.Web.Configuration.WebConfigurationManager.AppSettings["MailPassword"]))
        {
        }

        public ActionResult HttpError()
        {
            Exception ex = null;
            Boolean handled = false;

            try
            {
                ex = (Exception)HttpContext.Application[Request.UserHostAddress.ToString()];
            }
            catch (Exception exa)
            {
                ex = new Exception("Unable to pull exception details.");
            }

            // we want to ignore some errors, to prevent from being spammed ot all hell
            if (ex.Message.Contains("e8f6b078-0f35-11de-85c5-efc5ef23aa1f/aupm/notify.do") == true) { return View("Error"); }
            if (ex.Message.Contains("/crossdomain.xml") == true) { return View("Error"); }
            if (ex.Message.Contains("The connection id is in the incorrect format") == true) { return View("Error"); }

            if (ex.Message.Contains("The controller for path '/post/")) {
                    
                String path = Request.RawUrl.Replace("/error?aspxerrorpath=", "");

                Response.Redirect("http://crushes.tumblr.com" + path);
                handled = true;
            }

            Core.Model.Contact msg = new Core.Model.Contact();
            msg.Email = "seth.hayward@gmail.com";
            msg.Message = ex.Message.ToString();

            if(ex.InnerException != null) {
                msg.Message = msg.Message + "<br><br>" + ex.InnerException.Message.ToString();
                ViewData["Inner"] = ex.InnerException.Message.ToString();
            }

            if (ex.StackTrace != null)
            {
                msg.Message = msg.Message + "<br><br>Stack trace:<br/><code>" + ex.StackTrace.ToString() + "</code>";
            }

            if (handled == false)
            {
                _mailService.SendContact(msg.Message, msg.Email);                
            }

            if (ex != null)
            {
                ViewData["Description"] = ex.Message;
            }
            else
            {
                ViewData["Description"] = "An error occurred.";
            }

            ViewData["Title"] = "Oops. We're sorry. An error occurred and we're on the case.";       

            return View("Error");
        }

        public ActionResult Http404()
        {
            ViewData["Title"] = "The page you requested was not found";

            return View("Error");
        }

        // (optional) Redirect to home when /Error is navigated to directly
        public ActionResult Index()
        {
            return RedirectToAction("Index", "Home");
        }

    }
}
