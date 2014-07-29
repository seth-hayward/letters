using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Routing;
using System.Web.Mvc;
using System.Diagnostics;
using letterstocrushes.Models;
using System.Net.Mail;
using System.Web.Security;
using System.Web.Caching;
using letterstocrushes;
using StackExchange.Profiling;
using Microsoft.AspNet.SignalR;
using Microsoft.AspNet.SignalR.Client.Hubs;
using letterstocrushes.Core.Model;

namespace letterstocrushes.Controllers
{

    [HandleError]
    public class HomeController : Controller
    {

        public IFormsAuthenticationService FormsService { get; set; }
        public IMembershipService MembershipService { get; set; }
        private int _pagesize = 10;

#region Services

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

#endregion

        private readonly Core.Services.LetterService _letterService;
        private readonly Core.Services.EditService _editService;
        private readonly Core.Services.MailService _mailService;
        private readonly Core.Services.UserService _userService;
        private readonly Core.Services.CommentService _commentService;
        private readonly Core.Services.QueueService _queueService;
        private readonly Core.Services.VoteService _voteService;
        public HomeController(Core.Services.UserService userService,
                              Core.Services.MailService mailService,
                              Core.Services.EditService editService,
                              Core.Services.LetterService letterService,
                              Core.Services.CommentService commentService,
                              Core.Services.QueueService queueService,
                              Core.Services.VoteService voteService)
        {
            _userService = userService;
            _mailService = mailService;
            _editService = editService;
            _letterService = letterService;
            _commentService = commentService;
            _queueService = queueService;
            _voteService = voteService;

            if (FormsService == null) { FormsService = new FormsAuthenticationService(); }
            if (MembershipService == null) { MembershipService = new AccountMembershipService(); }
        }

        public HomeController() : this(new Core.Services.UserService(new Infrastructure.Data.EfQueryUsersByEmail()),
            new Core.Services.MailService(System.Web.Configuration.WebConfigurationManager.AppSettings["MailPassword"]),
            new Core.Services.EditService(new Infrastructure.Data.EfQueryEdits()),
            new Core.Services.LetterService(new Infrastructure.Data.EfQueryLetters(), new Core.Services.MailService(System.Web.Configuration.WebConfigurationManager.AppSettings["MailPassword"]), new Core.Services.BookmarkService(new Infrastructure.Data.EfQueryBookmarks()),
                new Core.Services.BlockService(new Infrastructure.Data.EfQueryBlocks())),
            new Core.Services.CommentService(new Infrastructure.Data.EfQueryLetters(), new Infrastructure.Data.EfQueryComments(), new Core.Services.MailService(System.Web.Configuration.WebConfigurationManager.AppSettings["MailPassword"]), new Core.Services.BlockService(new Infrastructure.Data.EfQueryBlocks())),
            new Core.Services.QueueService(new Infrastructure.Data.EfQueryQueue()),
            new Core.Services.VoteService(new Infrastructure.Data.EfQueryVotes()))
        {
        }

        #region Normal Pages

        [CompressFilter]
        public ActionResult Index(int page = 1, int mobile = 0)
        {

            try
            {

                string time_zone = getUserTimeZone();

                int _all_letter_count = 0;
                int _pages_db = 0;

                List<Core.Model.Letter> _letters = new List<Core.Model.Letter>();

                string cache_key_list = "home-page-" + page;
                string cache_key_count = "home-page-count-db1";

                //
                // Get total number of items in db. Insert into cache
                // if this number is not already there. This will
                // expire from cache in 90 seconds.
                //

                if (HttpContext.Cache[cache_key_count] == null)
                {

                    _all_letter_count = _letterService.getLetterCountHomePage();
                    HttpContext.Cache.Insert(cache_key_count, _all_letter_count, null, DateTime.UtcNow.AddSeconds(180), TimeSpan.Zero);
                }
                else
                {
                    _all_letter_count = (int)HttpContext.Cache[cache_key_count];

                }

                _pages_db = (int)Math.Ceiling((double)_all_letter_count / _pagesize);

                //
                // Store the actual list of letters in the cache
                //

                if (HttpContext.Cache[cache_key_list] == null)
                {

                    _letters = _letterService.getLetters(0, page, _pagesize).ToList();

                    double cacheTimeInSeconds = 30;

                    switch (page)
                    {
                        case 1:
                            cacheTimeInSeconds = 60 * 2;
                            break;
                        case 2:
                            cacheTimeInSeconds = 60 * 10;
                            break;
                        case 3:
                            cacheTimeInSeconds = 60 * 15;
                            break;
                        default:
                            cacheTimeInSeconds = 60 * 30;
                            break;
                    }

                    HttpContext.Cache.Insert(cache_key_list, _letters, null, DateTime.UtcNow.AddSeconds(cacheTimeInSeconds), TimeSpan.Zero);
                }
                else
                {
                    _letters = (List<Core.Model.Letter>)HttpContext.Cache[cache_key_list];
                }

                ViewData.Model = _letterService.fixList(_letters, time_zone);
                ViewBag.CurrentPage = page;
                ViewBag.Pages = _all_letter_count;


            } catch(Exception ex) {

                string foundErr = "index err: <br />" + ex.Message.ToString() + "<br />";

                if (ex.InnerException != null)
                {
                    foundErr = foundErr + "inner: " + ex.InnerException.Message.ToString();
                }

                _mailService.SendContact(foundErr, "seth.hayward@gmail.com");

            }

            if (mobile == 0)
            {
                return View();
            }
            else
            {
                return View("~/Views/Mobile/Index.cshtml");
            }
            
        }

        [CompressFilter]
        public ActionResult More(int page = 1, int mobile = 0)
        {

            string time_zone = getUserTimeZone();
            int _all_letter_count = 0;
            int _pages_db = 0;

            List<Core.Model.Letter> _letters = new List<Core.Model.Letter>();

            string cache_key_list = "more-page-" + page;
            string cache_key_count = "more-page-count-db";

            //
            // Fetch the total number of "more!" letters in db1.
            // Store this information in the cache for 90 seconds.
            // Fetch from cache if it exists there.
            //

            if (HttpContext.Cache[cache_key_count] == null)
            {
                _all_letter_count = _letterService.getLetterCountMorePage();
                HttpContext.Cache.Insert(cache_key_count, _all_letter_count, null, DateTime.UtcNow.AddSeconds(90), TimeSpan.Zero);
            }
            else
            {
                _all_letter_count = (int)HttpContext.Cache[cache_key_count];
            }

            _pages_db = (int)Math.Ceiling((double)_all_letter_count / _pagesize);

            //
            // Now... let's get our letters.
            // We're basically using offsets. 
            // We don't care about the ID. 
            // It is meaningless to us.
            //

            if (HttpContext.Cache[cache_key_list] == null)
            {
                _letters = _letterService.getLetters(-1, page, _pagesize).ToList();
                HttpContext.Cache.Insert(cache_key_list, _letters, null, DateTime.UtcNow.AddSeconds(15), TimeSpan.Zero);
            }
            else
            {
                _letters = (List<Core.Model.Letter>)HttpContext.Cache[cache_key_list];
            }

            ViewData.Model = _letterService.fixList(_letters, time_zone);
            ViewBag.CurrentPage = page;
            ViewBag.Pages = _all_letter_count;

            if (mobile == 0)
            {
                return View();
            }
            else
            {
                return View("~/Views/Mobile/More.cshtml");
            }

        }

        public ActionResult Apps()
        {
            return View();
        }

        public ActionResult ModLetters(int page = 1, int mobile = 0)
        {

            bool is_user_mod = User.IsInRole("Mod");
            if (is_user_mod == false)
            {
                return RedirectToAction("Index");
            }

            string time_zone = getUserTimeZone();
            int _all_letter_count = 0;
            int _pages_db = 0;

            List<Core.Model.Letter> _letters = new List<Core.Model.Letter>();

            string cache_key_list = "mod-page-" + page;
            string cache_key_count = "mod-page-count-db";

            //
            // Fetch the total number of "mod" letters in db1.
            // Store this information in the cache for 90 seconds.
            // Fetch from cache if it exists there.
            //

            if (HttpContext.Cache[cache_key_count] == null)
            {
                _all_letter_count = _letterService.getLetterCountModPage();
                HttpContext.Cache.Insert(cache_key_count, _all_letter_count, null, DateTime.UtcNow.AddSeconds(90), TimeSpan.Zero);
            }
            else
            {
                _all_letter_count = (int)HttpContext.Cache[cache_key_count];
            }

            _pages_db = (int)Math.Ceiling((double)_all_letter_count / _pagesize);

            //
            // Now... let's get our letters.
            // We're basically using offsets. 
            // We don't care about the ID. 
            // It is meaningless to us.
            //

            if (HttpContext.Cache[cache_key_list] == null)
            {
                _letters = _letterService.getModLetters(page, _pagesize);
                HttpContext.Cache.Insert(cache_key_list, _letters, null, DateTime.UtcNow.AddSeconds(15), TimeSpan.Zero);
            }
            else
            {
                _letters = (List<Core.Model.Letter>)HttpContext.Cache[cache_key_list];
            }

            ViewData.Model = _letterService.fixList(_letters, time_zone);
            ViewBag.CurrentPage = page;
            ViewBag.Pages = _all_letter_count;

            if (mobile == 0)
            {
                return View();
            }
            else
            {
                return View("~/Views/Mobile/More.cshtml");
            }

        }

        public ActionResult Archive(FormCollection fc, int year = 0, int month = 0, int day = 0, int page = 1, string terms = "")
        {

            DateTime today = DateTime.UtcNow;

            ViewBag.CurrentYear = today.Year;

            if (year == 0)
                year = today.Year;

            if (month == 0)
                month = today.Month;

            if (day == 0)
                day = today.Day;

            ViewBag.Results = 0;
            ViewBag.Terms = terms;
            
            ViewBag.Year = year;
            ViewBag.Month = month;
            ViewBag.Day = day;

            var profiler = MiniProfiler.Current;

            string cached_search_result = "archive-" + year.ToString() + "-" + month.ToString() + "-" + day.ToString() + "-terms-" + terms;

            List<Core.Model.Letter> results = new List<Core.Model.Letter>();

            string time_zone = getUserTimeZone();
            int tz = int.Parse(time_zone);

            if (HttpContext.Cache[cached_search_result] == null)
            {
                // not in cache, so we search db for it
                using (profiler.Step("SEARCH db1"))
                {
                    results = _letterService.searchByDate(terms, year, month, day, tz);
                }

                HttpContext.Cache.Insert(cached_search_result, results, null, DateTime.UtcNow.AddSeconds(90), TimeSpan.Zero);
            }
            else
            {
                // found the search results in the cache
                results = (List<Core.Model.Letter>)HttpContext.Cache[cached_search_result];
            }

            ViewBag.Results = results.Count();
            results = results.Skip((page - 1) * _pagesize).Take(_pagesize).ToList();

            if (ViewBag.Results == 0)
            {
                ViewBag.NothingFound = true;
            }
            else
            {
                ViewBag.NothingFound = false;
            }

            ViewBag.CurrentPage = page;
            double pages = Math.Ceiling((double)ViewBag.Results / 10);
            ViewBag.Pages = pages;

            ViewData.Model = _letterService.fixList(results, time_zone);

            return View();

        }

        [HttpGet]
        public ActionResult Terms()
        {
            return View();
        }

        [HttpGet]
        public ActionResult Privacy()
        {
            return View();
        }

        public ActionResult Details(int id = 1, int mobile = 0)
        {
            string time_zone = getUserTimeZone();

            Core.Model.Letter letterToView;

            letterToView = _letterService.getLetter(id, time_zone);

            if (letterToView.letterLevel == -10)
            {
                // mod letter, mod eyes only
                if (User.IsInRole("Mod") == false)
                {
                    return RedirectToAction("Index");
                }

            }

            HttpCookie letter_cookie = Request.Cookies[letterToView.letterTags];

            if (letterToView == null)
            {
                return RedirectToAction("NotFound");
            }
          
            ViewBag.display_pretty_box = false;
            ViewBag.can_edit = false;

            if (letter_cookie != null)
            {
                // if they have this cookie, we assume that they are
                // able to edit the letter. the cookie is placed on the
                // user's computer when they send a letter.
                ViewBag.can_edit = true;
            }

            if (User.IsInRole("Mod") == true)
            {
                // all mods can edit all letters
                ViewBag.can_edit = true;
            }

            if (User.Identity.IsAuthenticated == true)
            {
                ViewBag.display_pretty_box = true;
            }

            if (ViewBag.can_edit == true)
            {
                ViewBag.display_pretty_box = true;
            }

            ViewBag.comments = _commentService.getComments(id, false);

            if (User.Identity.IsAuthenticated == true)
            {

                string email = "";
                MailAddress test;
                try
                {
                    test = new MailAddress(User.Identity.Name);
                    email = test.Address;
                } catch (Exception ex) {
                }

                ViewBag.email = email;
            }

            string new_commenter_guid = null;
            new_commenter_guid = System.Guid.NewGuid().ToString();
            ViewBag.NewCommenterGuid = new_commenter_guid;

            ViewData.Model = letterToView;

            if (mobile == 0)
            {
                return View();
            }
            else
            {
                return View("~/Views/Mobile/Details.cshtml");
            }

        }

        [HttpPost]
        public ActionResult Details(FormCollection fc)
        {

            int letterId = int.Parse(fc["letterId"].ToString());
            string name = fc["commenterName"].ToString();
            string email = fc["commenterEmail"].ToString();
            string message = fc["comment"].ToString();
            bool using_mobile = false;
            bool mod_mode = false;
            
            if(fc["mod_mode"].Contains("true")) {
                mod_mode = true;
            } else {
                mod_mode = false;
            };
            
            if(fc["mobile"].ToString() == "1") {
                using_mobile = true;
            }


            string userip = string.Empty;
            userip = Request.ServerVariables["HTTP_X_FORWARDED_FOR"];
            if (userip == null || userip == "127.0.0.1")
                userip = Request.ServerVariables["REMOTE_ADDR"];


            Core.Model.Comment comm = new Core.Model.Comment();
            comm.commentDate = DateTime.UtcNow;
            comm.commenterEmail = email;
            comm.commenterName = name;
            comm.commentMessage = message;
            comm.letterId = letterId;
            comm.level = 0;
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

            if (mod_mode == true && User.IsInRole("mod"))
            {
                comm.commenterGuid = "mod" + current_guid.Substring(2, current_guid.Length - 3);
            }

            if (email != null)
            {
                comm.sendEmail = true;
            }

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

            if (using_mobile == true)
            {
                return RedirectToRoute("DetailsMobile", new { id = letterId, mobile = 1 });
            }
            else
            {
                return RedirectToAction("Details", new { id = letterId });
            }
        }

        [HttpGet]
        public JsonResult GetLetter(int id)
        {
            Core.Model.Letter letterToView;
            letterToView = _letterService.getLetter(id).Clone();

            letterToView.letterMessage = cleanHTMLElementsFromLetter(letterToView.letterMessage);

            return Json(letterToView, JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public JsonResult GetLetters(int page, int level)
        {
            List<Core.Model.Letter> letters = new List<Core.Model.Letter>();

            string cache_key_list = "mobile-" + level.ToString() + "-p" + page.ToString();

            if (HttpContext.Cache[cache_key_list] == null)
            {

                switch(level)
                {
                    case -10:
                        letters = _letterService.getModLetters(page, _pagesize).ToList();
                        break;
                    default:
                        letters = _letterService.getLetters(level, page, _pagesize).ToList();
                        break;
                }

                HttpContext.Cache.Insert(cache_key_list, letters, null, DateTime.UtcNow.AddSeconds(15), TimeSpan.Zero);
            }
            else
            {
                letters = (List<Core.Model.Letter>)HttpContext.Cache[cache_key_list];
            }

            return Json(letters, JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public JsonResult SearchLetters(string terms, int page)
        {
            List<Core.Model.Letter> letters = new List<Core.Model.Letter>();

            string cache_key_list = "mobile-search-" + terms;

            if (HttpContext.Cache[cache_key_list] == null)
            {
                letters = _letterService.search(terms).ToList();
                HttpContext.Cache.Insert(cache_key_list, letters, null, DateTime.UtcNow.AddSeconds(180), TimeSpan.Zero);
            }
            else
            {
                letters = (List<Core.Model.Letter>)HttpContext.Cache[cache_key_list];
            }

            letters = letters.Skip((page - 1) * 10).Take(10).ToList();

            return Json(letters, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public JsonResult facebookLetter(int id, long toFacebookUID, long fromFacebookUID)
        {
            int lucky_id = Convert.ToInt32(id);

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

            Core.Model.Letter lucky = _letterService.getLetter(lucky_id);

            HttpCookie check_cookie = Request.Cookies[lucky.letterTags];
            string check_value = null;
            if (check_cookie != null)
            {
                check_value = check_cookie.Value;
            }

            Boolean facebooked = _letterService.facebookLetter(lucky_id, toFacebookUID, fromFacebookUID, userip, check_value, user_name, is_user_mod);

            if (facebooked == true)
            {
                return Json(new { Result = 1, Message = "Letter facebooked." }, JsonRequestBehavior.AllowGet);
            }
            else
            {
                return Json(new { Result = 0, Message = "Failure to facebook. Unknown reason, could be anything really." }, JsonRequestBehavior.AllowGet);
            }

        }

        [HttpGet]
        public JsonResult hideLetter(int id)
        {

            int _response = 0;

            int lucky_id = Convert.ToInt32(id);

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

            Core.Model.Letter lucky = _letterService.getLetter(lucky_id);

            HttpCookie check_cookie = Request.Cookies[lucky.letterTags];
            string check_value = null;
            if (check_cookie != null)
            {
                check_value = check_cookie.Value;
            }

            Boolean hidden = _letterService.hideLetter(lucky_id, userip, check_value, user_name, is_user_mod);

            string msg = "";

            if (hidden == true)
            {
                _response = 1;
                msg = "Letter hidden.";
                msg += "This letter has been successfully hidden.";
            }
            else
            {
                _response = 0;
                msg = "Unable to hide letter.";
                msg += "The letter was not found or you are not authorized to make this change.";
            }

            return Json(new { response = _response, message = msg, guid = "0" }, JsonRequestBehavior.AllowGet);

        }

        public ActionResult Unsubscribe(string email, int id=0)
        {
            if (id == 0)
                return View();

            Core.Model.Letter unsubscribed_letter = _commentService.Unsubscribe(email, id);
            return View(unsubscribed_letter);
        }

        [CompressFilter]
        [HttpGet]
        public ActionResult Send()
        {

            string useragent = HttpContext.Request.UserAgent.ToLower();

            if(useragent.Contains("iphone") | useragent.Contains("ipod") | useragent.Contains("ipad") | useragent.Contains("android") | useragent.Contains("blackberry")) {
                return RedirectToAction("MobileSend");
            }

            return View();
            
        }

        public ActionResult MobileEdit(int id=1)
        {

            string time_zone_value = getUserTimeZone();
            Core.Model.Letter letterToView = _letterService.getLetter(id);

            if (letterToView == null)
            {
                return RedirectToAction("NotFound");
            }

            string letter_cookie_value = getLetterCookieValue(letterToView.letterTags);
            Boolean able_to_edit = _letterService.canEdit(letter_cookie_value, User.IsInRole("mod"));
            ViewBag.can_edit = able_to_edit;

            if (able_to_edit == false)
            {
                return View("Security");
            }

            // remove html elements from the letter message so it can be edited
            letterToView.letterMessage = cleanHTMLElementsFromLetter(letterToView.letterMessage);

            ViewData.Model = letterToView;
            return View("~/Views/Mobile/Edit.cshtml");
        }

        public ActionResult MobileSend()
        {
            return View("m");
        }

        public ActionResult Bookmarks()
        {
            return RedirectToAction("Index", "Account");
        }

        public ActionResult RSS()
        {
            return new EmptyResult();
        }

        [CompressFilter]
        public ActionResult Best()
        {

            string time_zone = getUserTimeZone();
            var profiler = MiniProfiler.Current;

            List<Core.Model.Letter> _letters = new List<Core.Model.Letter>();

            string best_cache_key = "best";

            if (HttpContext.Cache[best_cache_key] == null)
            {
                using (profiler.Step("BEST query"))
                {
                    _letters = _letterService.getLetters(1, 1, 1000);
                    HttpContext.Cache.Insert(best_cache_key, _letters, null, DateTime.UtcNow.AddMinutes(3), TimeSpan.Zero);
                }
            }
            else
            {
                _letters = (List<Core.Model.Letter>)HttpContext.Cache[best_cache_key];
            }

            ViewData.Model = _letters;

            return View();
        }

        [CompressFilter]
        [HttpGet]
        public ActionResult Contact()
        {
            return View();
        }

        [HttpPost]
        [ValidateInput(false)]
        public ActionResult Contact(Core.Model.Contact msg)
        {

            String userip = Request.ServerVariables["HTTP_X_FORWARDED_FOR"];
            if (userip == null || userip == "127.0.0.1")
                userip = Request.ServerVariables["REMOTE_ADDR"];

            msg.Message = "Feedback received ( " + msg.Email + " ): <br /><br />" + msg.Message + "<br><br>sent from ip: " + userip;
          
            _mailService.SendContact(msg.Message, msg.Email);

            Letter contact_letter = new Letter();
            contact_letter.letterLanguage = "en-US";
            contact_letter.letterLevel = -10;
            contact_letter.letterPostDate = DateTime.UtcNow;
            contact_letter.senderIP = userip;
            contact_letter.letterMessage = msg.Message;
            contact_letter.letterComments = 0;
            contact_letter.letterUp = 1;

            _letterService.AddLetter(contact_letter);

            return View("Thanks");
        }

        public ActionResult Edit(int id = 1)
        {
            string time_zone_value = getUserTimeZone();
            Core.Model.Letter letterToView = _letterService.getLetter(id);

            if (letterToView == null)
            {
                return RedirectToAction("NotFound");
            }

            string letter_cookie_value = getLetterCookieValue(letterToView.letterTags);
            Boolean able_to_edit = _letterService.canEdit(letter_cookie_value, User.IsInRole("mod"));
            ViewBag.can_edit = able_to_edit;

            if (able_to_edit == false)
            {
                return View("Security");
            }

            return View(letterToView);
        }

        public ActionResult Edits(int page=1)
        {
            string time_zone_value = getUserTimeZone();
            int _all_edits = _editService.getEditCount();
            List<Core.Model.Edit> edits = _editService.getEdits(page, _pagesize, time_zone_value);

            ViewData.Model = edits;
            ViewBag.CurrentPage = page;
            ViewBag.Pages = _all_edits;

            return View();
        }

        public ActionResult Search(FormCollection fc, int mobile = 0, int page = 1, string terms = "")
        {

            if (terms == "")
            {

                ViewBag.Results = 0;

                if (mobile == 0)
                {
                    return View();
                }
                else
                {
                    return View("~/Views/Mobile/Search.cshtml");
                }           

            }

            ViewBag.Results = 0;
            ViewBag.Terms = terms;

            var profiler = MiniProfiler.Current;

            string cached_search_result = "search-" + terms;

            List<Core.Model.Letter> results = new List<Core.Model.Letter>();

            if (HttpContext.Cache[cached_search_result] == null)
            {
                // not in cache, so we search db for it
                using (profiler.Step("SEARCH db1"))
                {
                    results = _letterService.search(terms);
                }

                HttpContext.Cache.Insert(cached_search_result, results, null, DateTime.UtcNow.AddSeconds(90), TimeSpan.Zero);
            }
            else
            {
                // found the search results in the cache
                results = (List<Core.Model.Letter>)HttpContext.Cache[cached_search_result];
            }

            ViewBag.Results = results.Count();
            results = results.Skip((page - 1) * _pagesize).Take(_pagesize).ToList();

            if (ViewBag.Results == 0)
            {
                ViewBag.NothingFound = true;
            }
            else
            {
                ViewBag.NothingFound = false;
            }

            ViewBag.CurrentPage = page;
            double pages = Math.Ceiling((double)ViewBag.Results / 10);
            ViewBag.Pages = pages;

            ViewData.Model = results;

            if (mobile == 0)
            {
                return View();
            }
            else
            {
                return View("~/Views/Mobile/Search.cshtml");
            }            
        }

        public ActionResult Queue()
        {
            if (User.IsInRole("Mod") == false) { return RedirectToAction("Index"); }
            
            List<Core.Model.Letter> queued_letters = _queueService.getQueuedLetters();
            ViewBag.Count = queued_letters.Count();

            string time_zone = getUserTimeZone();
            queued_letters = _letterService.fixList(queued_letters, time_zone);
            ViewData.Model = queued_letters;

            return View();
        }

        public ActionResult Dequeue(int id = 0)
        {
            _queueService.Dequeue(id);
            return RedirectToAction("queue");
        }

        public ActionResult PublishQueue()
        {
            Core.Model.Letter published = _queueService.PublishQueue();
            Core.Model.Contact msg = new Core.Model.Contact();

            if (published == null)
            {

                msg.Email = "noreply@letterstocrushes.com";
                msg.Message = "Tried to publish from queue but it was empty.";
                _mailService.SendContact(msg.Message,msg.Email);
            }
            else
            {
                msg.Email = "noreply@letterstocrushes.com";
                msg.Message = "Published:\r\n " + published.letterMessage;
                _mailService.SendContact(msg.Message, msg.Email);
            }
            return RedirectToAction("Index");
        }

        [HttpGet]
        public ActionResult Password(string id = "")
        {

            //
            // stage 1: enter email stage
            // stage 2: link sent
            // stage 3: enter new password
            // stage 4: success message (change password) --> redirects? logs that person in?
            // stage 5: fail message (handles multiple types)
            //
           
            if (id == "")
            {
                ViewBag.userguid = 0;
                ViewBag.Stage = 1;
            }
            else
            {
                ViewBag.Stage = 3;

                var lost_password = _userService.GetLostPassword(id);
                Guid user_guid = Guid.Parse(lost_password.userguid);
                var lost_user = _userService.getUserByGuid(user_guid);
                ViewBag.userguid = lost_user.UserId;
                ViewBag.Email = lost_user.UserName;
            }

            return View();

        }

        [HttpPost]
        public ActionResult Password(string email = "", int what = 1, string id = "", string pass = "")
        {

            try
            {

                Core.Model.Contact msg = new Core.Model.Contact();

                switch (what)
                {
                    // this is the default route,
                    // the user has just entered
                    // their email and we need to 
                    // make sure that there is 
                    // a user that exists for that
                    // email!

                    case 1:

                        var lost_user = _userService.getUserByEmail(email);

                        if (lost_user == null)
                        {

                            ViewBag.Stage = 5;
                            ViewBag.ErrorTitle = "unable to find that account.";
                            ViewBag.ErrorMessage = "we looked really hard, but we couldn't find that account.";

                        }
                        else
                        {

                            var password_change_request = _userService.AddLostPassword(lost_user.UserId.ToString());

                            msg.Email = email;

                            string message_body;
                            message_body = "Hello, \n\n" +
                                            "You requested a password reset. Please click this " +
                                            "link to change your password:\n" +
                                            "http://letterstocrushes.com/password/" + password_change_request.cguid + "\n\n" +
                                            "sincerely,\nletters to crushes";

                            msg.Message = message_body;

                            _mailService.SendPasswordLink(msg.Message, msg.Email);
                            ViewBag.Email = email;
                            ViewBag.Stage = 2;

                        }

                        break;
                    case 3:

                        // ok, we just received a new password.
                        // time to change it!

                        var lost_password = _userService.GetLostPassword(id);
                        Guid user_guid = Guid.Parse(lost_password.userguid);
                        lost_user = _userService.getUserByGuid(user_guid);

                        MembershipUser user = Membership.Provider.GetUser(lost_user.UserName, false);

                        // some users were getting locked out if they
                        // reset their passwords too much
                        if (user.IsLockedOut == true)
                        {
                            user.UnlockUser();
                        }

                        string newPassword = pass;
                        string tempPassword = user.ResetPassword();

                        user.ChangePassword(tempPassword, newPassword);

                        // now log the user in...

                        if (MembershipService.ValidateUser(lost_user.UserName, newPassword))
                        {
                            FormsService.SignIn(lost_user.UserName, true);
                            return RedirectToAction("Index", "Account", null);
                        }
                        else
                        {

                            // there was an error :(

                            ViewBag.Stage = 5;
                            ViewBag.ErrorTitle = "error! :(";
                            ViewBag.ErrorMessage = "there was an error signing in.";
                        }

                        break;
                }


            }
            catch (Exception ex)
            {

                Core.Model.Contact error_msg = new Core.Model.Contact();
                error_msg.Email = "change password";
                string message = "stage: " + what + "<br/>" + ex.Message.ToString();

                if (ex.InnerException != null)
                {
                    message = message + "<br /><br />" + ex.InnerException.ToString();
                }

                error_msg.Message = message;

                _mailService.SendContact(error_msg.Message, error_msg.Email);

            }
                            
            
            return View();

        }

        #endregion

        #region Admin Pages

        public ActionResult Popular(int page=1)
        {

          List<Core.Model.Letter> pagination = new List<Core.Model.Letter>();

            try
            {


              if (User.Identity.IsAuthenticated == false)
              {
                return RedirectToAction("Index", "Account");
              }

              if (User.IsInRole("Mod") == false)
                RedirectToAction("Index");


              string cache_latest_front_page = "latest_front_page";
              string cache_popular_page_count = "popular_page_count";

              // NEW PLAN: dynamic
              // i want to change how many letters i store on the meta pages.
              int meta_page_letters = 1000;
              int pages_per_meta_page = meta_page_letters / _pagesize;

              double current_popular_meta_page;
              double page_calc = page / pages_per_meta_page;
              current_popular_meta_page = Math.Ceiling(page_calc);

              double position_on_meta_page;

              // page     meta page       position
              // 1        1               1
              // 2        1               2
              // 3        1               3
              // 10       2               
              position_on_meta_page = (page % pages_per_meta_page);

              string cache_popular_page = "popular_page_" + current_popular_meta_page;

              //
              // Using the MVC Profiler to go wicked fast.
              //

              var profiler = MiniProfiler.Current;

              //
              // Fetch the latest front page letter.
              // Store this information in the cache for 1 hour.
              // Fetch from cache if it exists there.
              //

              Core.Model.Letter latest_front_page;

              if (HttpContext.Cache[cache_latest_front_page] == null)
              {

                using (profiler.Step("Getting latest front page"))
                {
                  latest_front_page = _letterService.getLatestFrontPageLetter();
                  HttpContext.Cache.Insert(cache_latest_front_page, latest_front_page, null, DateTime.UtcNow.AddHours(1), TimeSpan.Zero);
                }

              }
              else
              {
                latest_front_page = (Core.Model.Letter)HttpContext.Cache[cache_latest_front_page];
              }

              //
              // Fetch the requested popular page.
              // Store this information in the cache for 1 hour.
              // Fetch from cache if it exists there.
              //

              int popular_page_count = 0;

              if (HttpContext.Cache[cache_popular_page] == null)
              {

                using (profiler.Step("Getting popular page"))
                {

                  List<Core.Model.Letter> nada_surf = new List<Core.Model.Letter>();
                  nada_surf = _letterService.getPopularLetters(latest_front_page);

                  //
                  // store the count in cache if it's not there...
                  //

                  if (HttpContext.Cache[cache_popular_page_count] == null)
                  {
                    popular_page_count = nada_surf.Count;
                    HttpContext.Cache.Insert(cache_popular_page_count, popular_page_count, null, DateTime.UtcNow.AddHours(1), TimeSpan.Zero);
                  }

                  pagination = nada_surf.Skip((page - 1) * 100).Take(100).ToList();

                  HttpContext.Cache.Insert(cache_popular_page, nada_surf, null, DateTime.UtcNow.AddHours(1), TimeSpan.Zero);
                }
              }
              else
              {
                List<Core.Model.Letter> pre_pagination = (List<Core.Model.Letter>)HttpContext.Cache[cache_popular_page];
                pagination = pre_pagination.Skip((page - 1) * 100).Take(100).ToList();
              }


              ViewBag.CurrentPage = page;
              ViewBag.Pages = popular_page_count;

              ViewData.Model = _letterService.fixList(pagination, getUserTimeZone());

              return View();

            }
            catch (Exception ex)
            {
              String err = ex.Message;

              if (ex.InnerException != null)
              {
                err = err + "<br />" + ex.InnerException.Message.ToString();
              }
              _mailService.SendContact("Popular page: <br />" + err, "seth.hayward@gmail.com");

              return View(pagination);
            }

        }

        #endregion

        #region Supporting Functions

        [HttpGet]
        public ActionResult Hide(string id, int mobile = 0)
        {
            int lucky_id = Convert.ToInt32(id);

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

            Core.Model.Letter lucky = _letterService.getLetter(lucky_id);

            HttpCookie check_cookie = Request.Cookies[lucky.letterTags];
            string check_value = null;
            if (check_cookie != null)
            {
                check_value = check_cookie.Value;
            }

            Boolean hidden = _letterService.hideLetter(lucky_id, userip, check_value, user_name, is_user_mod);

            if (hidden == true)
            {
                ViewBag.Header = "Letter hidden.";
                ViewBag.Message = "This letter has been successfully hidden.";
            }
            else
            {
                ViewBag.Header = "Unable to hide letter.";
                ViewBag.Message = "The letter was not found or you are not authorized to make this change.";
            }

            if (mobile == 0)
            {
                return View("Hidden");
            }
            else
            {
                return View("~/Views/Mobile/Hidden.cshtml");
            }

        }

        [HttpGet]
        public ActionResult Unhide(string id, int mobile = 0)
        {
            int lucky_id = Convert.ToInt32(id);

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

            Core.Model.Letter lucky = _letterService.getLetter(lucky_id);

            HttpCookie check_cookie = Request.Cookies[lucky.letterTags];
            string check_value = null;
            if (check_cookie != null)
            {
                check_value = check_cookie.Value;
            }

            Boolean hidden = _letterService.unhideLetter(lucky_id, userip, check_value, user_name, is_user_mod);

            if (hidden == true)
            {
                ViewBag.Header = "Letter shown again.";
                ViewBag.Message = "This letter will now appear on the more pages again.";
            }
            else
            {
                ViewBag.Header = "Unable to unhide letter.";
                ViewBag.Message = "The letter was not found or you are not authorized to make this change.";
            }

            if (mobile == 0)
            {
                return View("Hidden");
            }
            else
            {
                return View("~/Views/Mobile/Hidden.cshtml");
            }

        }

        [HttpGet]
        public ActionResult AddToQueue(string id)
        {

            if (User.IsInRole("Mod") == false) { return RedirectToAction("Security"); }
            int lucky_id = Convert.ToInt32(id);

            _queueService.Queue(lucky_id, User.Identity.Name.ToString());

            return RedirectToAction("Queue");
        }

        [HttpPost]
        [ValidateInput(false)]
        public JsonResult Mail(string letterText, string letterCountry, string mobile = "0")
        {

            String error_message = string.Empty;
            string userip = string.Empty;
            userip = Request.ServerVariables["HTTP_X_FORWARDED_FOR"];
            if (userip == null || userip == "127.0.0.1")
                userip = Request.ServerVariables["REMOTE_ADDR"];

            string userid = null;

            if (User.Identity.IsAuthenticated == true)
            {
                MembershipUser MemUser = Membership.GetUser();
                userid = MemUser.ProviderUserKey.ToString();
            }

            Core.Model.Letter letter;
            
            try
            {
                letter = _letterService.Mail(Server.HtmlDecode(letterText), letterCountry, userip, userid, int.Parse(mobile), ref error_message);

                if (User.Identity.IsAuthenticated == true && letter != null)
                {
                    // add an invisible comment so that the user will receive email notifications

                    Comment invisa_comment = new Comment();
                    invisa_comment.level = -2;
                    invisa_comment.letterId = letter.Id;
                    invisa_comment.sendEmail = true;
                    invisa_comment.commentDate = DateTime.UtcNow;
                    invisa_comment.commenterEmail = User.Identity.Name;
                    invisa_comment.commenterGuid = System.Guid.NewGuid().ToString();
                    invisa_comment.commenterName = "";

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

                    _commentService.AddComment(invisa_comment, host);

                }
            }
            catch (Exception ex) {
                letter = null;
                error_message = ex.Message;
            }

            // remove the more page
            // and mod page cache objects so they
            // get pulled again immedately with the new data
            HttpContext.Cache.Remove("mod-page-1");
            HttpContext.Cache.Remove("more-page-1");

            if (letter != null)
            {
                return Json(new { response = 1, message = letter.Id, guid = letter.letterTags }, JsonRequestBehavior.AllowGet);
            }
            else
            {
                return Json(new { response = 0, message = error_message, guid = "Error in response" }, JsonRequestBehavior.AllowGet);
            }
        }

        [HttpPost]
        [ValidateInput(false)]
        public JsonResult EditLetter(string letterText, string id, string mobile = "0")
        {

            Boolean edit_successful = false;
            String fail_message = "";

            try
            {

                int lucky_id = Convert.ToInt32(id);

                if (mobile == "1")
                {
                    letterText = "<p>" + letterText;
                    letterText = letterText.Replace(System.Environment.NewLine, "</p><p>");
                    letterText = letterText + "</p>";
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

                Core.Model.Letter lucky = _letterService.getLetter(lucky_id);

                HttpCookie check_cookie = Request.Cookies[lucky.letterTags];
                string check_value = null;
                if (check_cookie != null)
                {
                    check_value = check_cookie.Value;
                }

                edit_successful = _letterService.editLetter(lucky_id, Server.HtmlDecode(letterText), userip, check_value, user_name, is_user_mod, int.Parse(mobile));

            }
            catch (Exception ex)
            {
                fail_message = ex.Message;
                _mailService.SendContact("Edit error: <br />" + ex.Message, "seth.hayward@gmail.com");
            }

            if (edit_successful == true)
            {
                return Json(new { response = 1, message = "Success." }, JsonRequestBehavior.AllowGet);
            }
            else
            {
                return Json(new { response = 0, message = "Unable to edit letter: " + fail_message }, JsonRequestBehavior.AllowGet);
            }
        }

        public JsonResult Vote(int id, string c = "")
        {

            string userip = null;
            userip = Request.ServerVariables["HTTP_X_FORWARDED_FOR"];
            if (userip == null)
                userip = Request.ServerVariables["REMOTE_ADDR"];

            string signalr_hub = "http://localhost:" + Request.Url.Port + VirtualPathUtility.ToAbsolute("~/signalr/hubs");

            Letter letter = new Letter();

            letter.letterMessage = "Testing.";

            try
            {
                int current_upvotes = _voteService.Vote(id, userip);

                letter.letterUp = current_upvotes;

                //// announce the vote to the voting realtime service
                //if (c != "")
                //{

                //    // create a new connection if one does not exist
                //    if (hubconnection == null || hubconnection.state == microsoft.aspnet.signalr.client.connectionstate.disconnected)
                //    {
                //        hubconnection = new hubconnection(signalr_hub);

                //        // create a proxy to the chat service
                //        hub = hubconnection.createhubproxy("visitorupdate");

                //        // start the connection
                //        hubconnection.start().wait();

                //    }


                //    if (hubconnection.state == microsoft.aspnet.signalr.client.connectionstate.connected)
                //    {

                //        // send the message
                //        hub.invoke("vote", id, c);

                //        hubconnection.stop();
                //    }

                //}

                return Json(letter, JsonRequestBehavior.AllowGet);

            }
            catch (Exception ex)
            {

                Core.Model.Contact msg = new Core.Model.Contact();
                msg.Email = "seth.hayward@gmail.com";


                string hub_state = "";
                if (hubConnection != null)
                {
                    hub_state = hubConnection.State.ToString();
                }
                else
                {
                    hub_state = "null";
                }
                msg.Message = "state: " + hub_state + "<br><br>" + ex.Message.ToString();
                if (ex.InnerException != null)
                {
                    msg.Message = msg.Message + "<br><br>" + ex.InnerException.Message.ToString();
                }

                _mailService.SendContact(msg.Message, msg.Email);

            }        

            return Json(letter, JsonRequestBehavior.AllowGet);
            
        }

        public string cleanHTMLElementsFromLetter(string letter_message)
        {

            // now, we need to remove/replace all html elements
            // so that we can edit the device on iOS...
            // elements to replace:
            //    - <ul> </ul>
            //    - <li> </li>
            //    - <p> </p>
            //    - <b> </b>
            //    - <i> </i>

            Dictionary<string, string> ignored_html_elements_with_replacements = new Dictionary<string, string>();
            ignored_html_elements_with_replacements.Add("<ul>", "");
            ignored_html_elements_with_replacements.Add("</ul>", "");
            ignored_html_elements_with_replacements.Add("<p>", "");
            ignored_html_elements_with_replacements.Add("</p>", "\n");
            ignored_html_elements_with_replacements.Add("<i>", "");
            ignored_html_elements_with_replacements.Add("</i>", "");
            ignored_html_elements_with_replacements.Add("<b>", "");
            ignored_html_elements_with_replacements.Add("</b>", "");

            foreach (KeyValuePair<string, string> oops in ignored_html_elements_with_replacements)
            {
                letter_message = letter_message.Replace(oops.Key, oops.Value);
            }

            return letter_message;

        }

        public string getUserTimeZone()
        {
            HttpCookie time_zone;
            string time_zone_value = null;

            try
            {
                time_zone = Request.Cookies["userTimeZone"];

                if (time_zone == null)
                {
                    time_zone_value = "0";
                }
                else
                {
                    time_zone_value = time_zone.Value;
                }

            }
            catch (Exception ex) {
                time_zone_value = "0";
            }
                
            return time_zone_value;
        }

        public string getLetterCookieValue(string tags)
        {
            HttpCookie cookie = Request.Cookies[tags];
            string value = null;

            if (cookie == null)
            {
                value = "";
            }
            else
            {
                value = cookie.Value;
            }

            return value;
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

        #endregion

    }
}
 