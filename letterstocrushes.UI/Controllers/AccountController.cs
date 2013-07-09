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

namespace letterstocrushes.Controllers
{
    public class AccountController : Controller
    {

        public IFormsAuthenticationService FormsService { get; set; }
        public IMembershipService MembershipService { get; set; }

        private readonly Core.Services.LetterService _letterService;
        private readonly Core.Services.BookmarkService _bookmarkService;

        public AccountController(Core.Services.LetterService letterService,
                                 Core.Services.BookmarkService bookmarkService)
        {
            _letterService = letterService;
            _bookmarkService = bookmarkService;
        }

        public AccountController()
            : this(new Core.Services.LetterService(new Infrastructure.Data.EfQueryLetters(), new Core.Services.MailService(System.Web.Configuration.WebConfigurationManager.AppSettings["MailPassword"]), new Core.Services.BookmarkService(new Infrastructure.Data.EfQueryBookmarks())),
                   new Core.Services.BookmarkService(new Infrastructure.Data.EfQueryBookmarks()))
        {
        }

        protected override void Initialize(RequestContext requestContext)
        {
            if (FormsService == null) { FormsService = new FormsAuthenticationService(); }
            if (MembershipService == null) { MembershipService = new AccountMembershipService(); }

            base.Initialize(requestContext);
        }


        //
        // BOOKMARKS BOOKMARKS BOOKMARKS
        //

        #region Bookmarks
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

        // **************************************
        // URL: /Account/LogOff
        // **************************************

        public ActionResult Logout(int mobile = 0)
        {
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

        public ActionResult Register()
        {
            ViewBag.PasswordLength = MembershipService.MinPasswordLength;
            return View();
        }

        [HttpPost]
        public ActionResult Register(RegisterModel model)
        {

            //Debug.Print("ModelState before: " + ModelState.IsValid.ToString());
            model.Email = model.UserName;
            //Debug.Print("ModelState after: " + ModelState.IsValid.ToString());

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
                    FormsService.SignIn(model.UserName, false /* createPersistentCookie */);
                    return RedirectToAction("Index", "Home");
                }
                else
                {
                    //Debug.Print("Error: " + AccountValidation.ErrorCodeToString(createStatus));
                    ModelState.AddModelError("", AccountValidation.ErrorCodeToString(createStatus));
                }
            }
            else {
                ModelState.AddModelError("", "there was an error.");
            }

            //Debug.Print("ModelState: " + ModelState.IsValid.ToString());
            //Debug.Print("Email: " + model.Email.ToString());
            //Debug.Print("UserName: " + model.UserName.ToString());
            //Debug.Print("Test email: " + TestEmail(model.Email.ToString()));

            // If we got this far, something failed, redisplay form
            ViewBag.PasswordLength = MembershipService.MinPasswordLength;
            return View(model);
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
                Debug.Print("Response: " + response.ToString());
            }
            else {
                Debug.Print("Null failure jesus christ.");
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

    }
}
