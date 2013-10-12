using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Security.Principal;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using System.Web.Security;
using letterstocrushes.Models;
using System.Diagnostics;
using System.Text.RegularExpressions;
using StackExchange.Profiling;
using Microsoft.AspNet.SignalR.Client.Hubs;

namespace letterstocrushes.Controllers
{
    public class AccountController : Controller
    {

        public IFormsAuthenticationService FormsService { get; set; }
        public IMembershipService MembershipService { get; set; }

        private readonly Core.Services.LetterService _letterService;
        private readonly Core.Services.BookmarkService _bookmarkService;
        private readonly Core.Services.MailService _mailService;

        public AccountController(Core.Services.LetterService letterService,
                                 Core.Services.BookmarkService bookmarkService,
                                 Core.Services.MailService mailService)
        {
            _letterService = letterService;
            _bookmarkService = bookmarkService;
            _mailService = mailService;
        }

        public AccountController()
            : this(new Core.Services.LetterService(new Infrastructure.Data.EfQueryLetters(), new Core.Services.MailService(System.Web.Configuration.WebConfigurationManager.AppSettings["MailPassword"]), new Core.Services.BookmarkService(new Infrastructure.Data.EfQueryBookmarks()),
                new Core.Services.BlockService(new Infrastructure.Data.EfQueryBlocks())),
                   new Core.Services.BookmarkService(new Infrastructure.Data.EfQueryBookmarks()), new Core.Services.MailService(System.Web.Configuration.WebConfigurationManager.AppSettings["MailPassword"]))
        {
        }

        protected override void Initialize(RequestContext requestContext)
        {
            if (FormsService == null) { FormsService = new FormsAuthenticationService(); }
            if (MembershipService == null) { MembershipService = new AccountMembershipService(); }

            base.Initialize(requestContext);
        }

        private HubConnection _hubConnection;
        private IHubProxy _hub;
        public HubConnection hubConnection
        {
            get
            {
                return _hubConnection;
            }
            set
            {
                _hubConnection = value;
            }
        }
        public IHubProxy hub
        {
            get
            {
                return _hub;
            }
            set
            {
                _hub = value;
            }
        }


        //
        // BOOKMARKS BOOKMARKS BOOKMARKS
        //

        #region Bookmarks

        [HttpGet]
        public JsonResult GetBookmarks(int id)
        {
            string UserID = "";
            MembershipUser MemUser = Membership.GetUser();
            UserID = MemUser.ProviderUserKey.ToString();

            List<Core.Model.Letter> bookmarks = new List<Core.Model.Letter>();

            foreach (Core.Model.Letter b in bookmarks)
            {
                b.fromFacebookUID = 0;
                b.toFacebookUID = 0;
            }

            bookmarks = _bookmarkService.getBookmarksByUser(UserID, id);
            bookmarks = _letterService.fixList(bookmarks, "0");

            return Json(bookmarks, JsonRequestBehavior.AllowGet);
        }

        public ActionResult Index(int page=1, int mobile=0)
        {

            if (User.Identity.IsAuthenticated == false)
            {
                if (mobile == 0)
                {
                    return RedirectToAction("Login", "Account");
                }
                else
                {
                    return RedirectToRoute("loginRouteMobile", null);
                }
            }

            string UserID = "";
            MembershipUser MemUser = Membership.GetUser();
            UserID = MemUser.ProviderUserKey.ToString();

            //
            // Profiler helps us go fast.
            //

            var profiler = MiniProfiler.Current;

            //
            // Fetch bookmarks from both of the databases.
            //

            int all_bookmarks = 0;
            List<Core.Model.Letter> bookmarks = new List<Core.Model.Letter>();
            string time_zone = getUserTimeZone();          

            using (profiler.Step("Fetching bookmarks from db1"))
            {
                all_bookmarks = _bookmarkService.getBookmarkCountByUser(UserID);
                bookmarks = _bookmarkService.getBookmarksByUser(UserID, page);
            }

            bookmarks = _letterService.fixList(bookmarks, time_zone);
            int lowest_id = 0;
            int highest_id = 0;

            foreach (Core.Model.Letter check in bookmarks)
            {
                if (lowest_id >= check.Id || lowest_id == 0)
                {
                    lowest_id = check.Id;
                }
                if (highest_id <= check.Id)
                {
                    highest_id = check.Id;
                }
            }
            ViewBag.Highest = highest_id;
            ViewBag.Lowest = lowest_id;
            ViewBag.CurrentPage = page;
            ViewBag.Pages = all_bookmarks;

            //
            // Display some stats... :)
            //

            if (User.Identity.Name == "seth" & page == 1)
            {
                ViewBag.db1 = _letterService.getLetterCount();
            }

            ViewData.Model = bookmarks;

            if (mobile == 0)
            {
                return View();
            }
            else
            {
                return View("~/Views/Mobile/Bookmarks.cshtml");
            }
                        
          }

          public ActionResult Bookmark(string id)
          {

              int response = 0;
              string response_str = " ";

              try
              {

                  string UserID = "";
                  MembershipUser MemUser = Membership.GetUser();
                  UserID = MemUser.ProviderUserKey.ToString();

                  _bookmarkService.Add(Convert.ToInt32(id), UserID);

                  response = 1;

              }
              catch (Exception err)
              {
                  response = -1;
                  response_str = "An error occurred on the server. (" + err.Message.ToString() + ")";

              }
              finally
              {

                  switch (response)
                  {
                      case 0:
                          response_str = "Incomplete.";
                          break;
                      case 1:
                          response_str = "Success.";
                          break;
                  }

              }

              return Json(new { Code = response, Message = response_str }, JsonRequestBehavior.AllowGet);


          }

          public JsonResult HideBookmark(string id)
          {

              int response = -1;
              string response_str = " ";

              try
              {

                  string UserID = "";
                  MembershipUser MemUser = Membership.GetUser();
                  UserID = MemUser.ProviderUserKey.ToString();

                  int bookmark_key = Convert.ToInt32(id);

                  Boolean result = _bookmarkService.Hide(bookmark_key, UserID);

                  if (result == true)
                  {
                      response = 1;
                  }
                  else
                  {
                      throw new Exception("Couldn't find the bookmark (id: " + id + ").");
                  }

              }
              catch (Exception err)
              {
                  response_str = err.Message;
              }

              switch (response)
              {
                  case 1:
                      response_str = "Your bookmark has been hidden.";
                      break;
                  case -1:
                      response_str = "There was an error. (" + response_str + ")";
                      break;
              }

              return Json(new {Code = response, Message = response_str}, JsonRequestBehavior.AllowGet);
          }

        #endregion


     
        // **************************************
        // URL: /Account/LogOn
        // **************************************

        public ActionResult Login(int mobile = 0)
        {
            if (mobile == 0)
            {
                return View();
            }
            else
            {
                return View("~/Views/Mobile/Login.cshtml");
            }
        }

        [HttpPost]
        public ActionResult Login(LogOnModel model, string returnUrl)
        {
            if (ModelState.IsValid)
            {
                if (MembershipService.ValidateUser(model.UserName, model.Password))
                {
                    FormsService.SignIn(model.UserName, true);
                    FormsAuthentication.SetAuthCookie(model.UserName, true);
                    
                    FormsAuthentication.RedirectFromLoginPage(model.UserName, true);

                    announceStatusChange();


                    if (model.Mobile == 1)
                    {
//                        return RedirectToAction("Index", "Account", new { mobile = 1 });
                        return RedirectToRoute("BookmarksRouteMobile");
                    }
                    else
                    {
                        return RedirectToAction("Index", "Account", null);
                    }

                    //
                    // I do not understand the purpose for the next check...?
                    //
      
                    //if (Url.IsLocalUrl(returnUrl))
                    //{
                    //    return Redirect(returnUrl);
                    //}
                    //else
                    //{

						//FormsAuthentication.RedirectFromLoginPage(model.UserName, true);					
                        // I am not sure if this is the correct object to 
						// call. It may simplyi e FormsService.RedirectFromLoginPage()
                }
                else
                {
                    ModelState.AddModelError("", "Email or password is incorrect. :(");
                }
            }


            // If we got this far, something failed, redisplay form
            ViewData.Model = model;

            if (model.Mobile == 1)
            {
                return View("~/Views/Mobile/Login.cshtml");
            }
            else
            {
                return View();
            }

        }


        [HttpPost]
        public JsonResult MobileLogin(string a, string b)
        {
            int result = 0;
            string is_mod = "0";

            if (MembershipService.ValidateUser(a, b))
            {
                FormsService.SignIn(a, true);
                FormsAuthentication.SetAuthCookie(a, true);
                announceStatusChange();

                if (User.IsInRole("Mod") == true)
                {
                    is_mod = "1";
                }
                else
                {
                    is_mod = User.Identity.Name;
                }
                result = 1;
            }
            else
            {
                result = 2;
            }

            return Json(new { message = result, response = result, guid = is_mod }, JsonRequestBehavior.AllowGet);
        }


        [HttpPost]
        public JsonResult MobileRegister(string a, string b)
        {
            int result = 0;
            string result_message = "";
          
            if (TestEmail(a) == false) {

                result = 200;
                result_message = "Please enter a valid email address.";

            } else {

                MembershipCreateStatus createStatus = MembershipService.CreateUser(a, b, a);

                switch(createStatus)
                {
                    case MembershipCreateStatus.Success:
                        FormsService.SignIn(a, true);
                        result = 1;
                        _mailService.SendContact("Mobile user joined: " + a, "seth.hayward@gmail.com");
                        break;
                    case MembershipCreateStatus.DuplicateUserName:
                        result_message = "Someone has already created an account with this address.";
                        result = 201;
                        break;
                    default:
                        _mailService.SendContact("Mobile register. " + a + ": <br />" + AccountValidation.ErrorCodeToString(createStatus), "seth.hayward@gmail.com");
                        break;
                }

            }

            return Json(new { message = result_message, response = result, guid = result }, JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public JsonResult MobileStatus()
        {
            int result = 0;
            int mod = 0;

            if (User.Identity.IsAuthenticated == true)
            {
                result = 1;

                if (User.IsInRole("Mod") == true)
                {
                    mod = 1;
                }
            }
            else
            {
                result = 0;
            }

            return Json(new { message = result.ToString(), response = result, guid = mod.ToString() }, JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public JsonResult MobileLogout()
        {
            announceStatusChange();
            FormsService.SignOut();
            return Json(new { message = "1", response = 1, guid = "1" }, JsonRequestBehavior.AllowGet);
        }

        // **************************************
        // URL: /Account/LogOff
        // **************************************

        public ActionResult Logout(int mobile = 0)
        {

            announceStatusChange();

            FormsService.SignOut();

            if (mobile == 0)
            {
                return RedirectToRoute("HomePages", new { page = 1 });
            }
            else
            {
                return RedirectToRoute("DefaultMobile");
            }

        }

        // **************************************
        // URL: /Account/Register
        // **************************************

        public ActionResult Register(int mobile = 0)
        {
            ViewBag.PasswordLength = MembershipService.MinPasswordLength;

            if (mobile == 0)
            {
                return View();
            }
            else
            {
                return View("~/Views/Mobile/Register.cshtml");
            }
        }

        [HttpPost]
        public ActionResult Register(RegisterModel model)
        {

            model.Email = model.UserName;

            if (TestEmail(model.Email) == false)
            {
                ModelState.AddModelError("UserName", "please enter a valid email address.");
            }

            if (ModelState.IsValid)
            {
                // Attempt to register the user
                MembershipCreateStatus createStatus = MembershipService.CreateUser(model.UserName, model.Password, model.Email);

                if (createStatus == MembershipCreateStatus.Success)
                {
                    FormsService.SignIn(model.UserName, true /* createPersistentCookie */);
                    _mailService.SendContact("New user joined: " + model.Email, "seth.hayward@gmail.com");
                    
                    if (model.Mobile == 0)
                    {
                        return RedirectToAction("Index", "Account", null);
                    }
                    else
                    {
                        return RedirectToRoute("BookmarksRouteMobile");
                    }
                }
                else
                {
                    ModelState.AddModelError("", AccountValidation.ErrorCodeToString(createStatus));
                }
            }
            else {
                ModelState.AddModelError("", "there was an error.");
            }


            // If we got this far, something failed, redisplay form
            ViewBag.PasswordLength = MembershipService.MinPasswordLength;
            ViewData.Model = model;

            if (model.Mobile == 0)
            {
                return View();
            }
            else
            {
                return View("~/Views/Mobile/Register.cshtml");
            }

        }

        // **************************************
        // URL: /Account/ChangePassword
        // **************************************

        [Authorize]
        public ActionResult ChangePassword()
        {
            ViewBag.PasswordLength = MembershipService.MinPasswordLength;
            return View();
        }

        [Authorize]
        [HttpPost]
        public ActionResult ChangePassword(ChangePasswordModel model)
        {
            if (ModelState.IsValid)
            {
                if (MembershipService.ChangePassword(User.Identity.Name, model.OldPassword, model.NewPassword))
                {
                    return RedirectToAction("ChangePasswordSuccess");
                }
                else
                {
                    ModelState.AddModelError("", "The current password is incorrect or the new password is invalid.");
                }
            }

            // If we got this far, something failed, redisplay form
            ViewBag.PasswordLength = MembershipService.MinPasswordLength;
            return View(model);
        }

        // **************************************
        // URL: /Account/ChangePasswordSuccess
        // **************************************

        public ActionResult ChangePasswordSuccess()
        {
            return View();
        }


        public bool TestEmail(string test)
        {

            //string patternStrict = @"^(([^<>()[\]\\.,;:\s@\""]+"
            //        + @"(\.[^<>()[\]\\.,;:\s@\""]+)*)|(\"".+\""))@"
            //        + @"((\[[0-9]{1,3}\.[0-9]{1,3}\.[0-9]{1,3}"
            //        + @"\.[0-9]{1,3}\])|(([a-zA-Z\-0-9]+\.)+"
            //        + @"[a-zA-Z]{2,}))$";

            string patternStrict = @"\w+([-+.]\w+)*@\w+([-.]\w+)*\.\w+([-.]\w+)*";
            Regex reStrict = new Regex(patternStrict);

            bool response = false;

            if (test != null) {
                bool isStrictMatch = reStrict.IsMatch(test);
                response = isStrictMatch;
            }
            else {
                response = false;
            }

            return response;

        }

        public string getUserTimeZone()
        {
            HttpCookie time_zone = Request.Cookies["userTimeZone"];
            string time_zone_value = null;

            if (time_zone == null)
            {
                time_zone_value = "0";
            }
            else
            {
                time_zone_value = time_zone.Value;
            }

            return time_zone_value;
        }

        public void announceStatusChange()
        {

            string userip = null;
            userip = Request.ServerVariables["HTTP_X_FORWARDED_FOR"];
            if (userip == null)
                userip = Request.ServerVariables["REMOTE_ADDR"];

            string signalr_hub = "http://localhost:" + Request.Url.Port + VirtualPathUtility.ToAbsolute("~/signalr/hubs");

            try
            {
                    // create a new connection if one does not exist
                    if (hubConnection == null || hubConnection.State == Microsoft.AspNet.SignalR.Client.ConnectionState.Disconnected)
                    {
                        hubConnection = new HubConnection(signalr_hub);

                        // Create a proxy to the chat service
                        hub = hubConnection.CreateHubProxy("visitorUpdate");

                        // Start the connection
                        hubConnection.Start().Wait();

                    }


                    if (hubConnection.State == Microsoft.AspNet.SignalR.Client.ConnectionState.Connected)
                    {

                        // Send the message

                        String user_ip = Request.UserHostAddress;

                        hub.Invoke("AnnounceStatusChange", user_ip);

                        hubConnection.Stop();
                    }
            }
            catch (Exception ex)
            {

                Core.Model.Contact msg = new Core.Model.Contact();


                string hub_state = "";
                if (hubConnection != null)
                {
                    hub_state = hubConnection.State.ToString();
                }
                else
                {
                    hub_state = "null";
                }
                msg.Message = "account controller. <br />state: " + hub_state + "<br><br>" + ex.Message.ToString();
                if (ex.InnerException != null)
                {
                    msg.Message = msg.Message + "<br><br>" + ex.InnerException.Message.ToString();
                }

                _mailService.SendContact(msg.Message, msg.Email);

            }


        }


    }

}
