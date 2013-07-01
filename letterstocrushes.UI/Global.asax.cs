using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using letterstocrushes;
using StackExchange.Profiling;

namespace letterstocrushes
{
    // Note: For instructions on enabling IIS6 or IIS7 classic mode, 
    // visit http://go.microsoft.com/?LinkId=9394801

    public class MvcApplication : System.Web.HttpApplication
    {
        public static void RegisterGlobalFilters(GlobalFilterCollection filters)
        {
            filters.Add(new HandleErrorAttribute());
        }

        public static void RegisterRoutes(RouteCollection routes)
        {
            routes.IgnoreRoute("{resource}.axd/{*pathInfo}");

            routes.MapRoute(
                "HomePages", // Route name
                "page/{page}", // URL with parameters
                new { controller = "Home", action = "Index" } // Parameter defaults
            );

            routes.MapRoute(
                "More", // Route name
                "more/", // URL with parameters
                new { controller = "Home", action = "More", id = UrlParameter.Optional } // Parameter defaults
            );

            routes.MapRoute(
                "MorePages", // Route name
                "more/page/{page}", // URL with parameters
                new { controller = "Home", action = "More" } // Parameter defaults
            );

            routes.MapRoute(
                "Comments", // Route name
                "comments/", // URL with parameters
                new { controller = "Comment", action = "Index" } // Parameter defaults
            );

            routes.MapRoute(
                "CommentsEdit", // Route name
                "comments/edit", // URL with parameters
                new { controller = "Comment", action = "Edit" } // Parameter defaults
            );

            routes.MapRoute(
                "CommentDetails", // Route name
                "comment/{id}", // URL with parameters
                new { controller = "Comment", action = "Details" } // Parameter defaults
            );

            routes.MapRoute(
                "CommentHide", // Route name
                "comment/{id}/hide", // URL with parameters
                new { controller = "Comment", action = "Hide" } // Parameter defaults
            );


            routes.MapRoute(
                "CommentPages", // Route name
                "comments/page/{page}", // URL with parameters
                new { controller = "Comment", action = "Index" } // Parameter defaults
            );

            routes.MapRoute(
                "CommentUnsubscribe", // Route name
                "unsubscribe/{id}/{email}", // URL with parameters
                new { controller = "Home", action = "Unsubscribe" } // Parameter defaults
            );

            routes.MapRoute(
                "Send", // Route name
                "send/", // URL with parameters
                new { controller = "Home", action = "Send" } // Parameter defaults
            );

            routes.MapRoute(
                "Search", // Route name
                "search/", // URL with parameters
                new { controller = "Home", action = "Search" } // Parameter defaults
            );

            routes.MapRoute(
                "Popular", // Route name
                "popular/", // URL with parameters
                new { controller = "Home", action = "Popular" } // Parameter defaults
            );

            routes.MapRoute(
                "Edits", // Route name
                "edits/", // URL with parameters
                new { controller = "Home", action = "Edits" } // Parameter defaults
            );

            routes.MapRoute(
                "Queue", // Route name
                "queue/", // URL with parameters
                new { controller = "Home", action = "Queue" } // Parameter defaults
            );

            routes.MapRoute(
                "Details", // Route name
                "letter/{id}", // URL with parameters
                new { controller = "Home", action = "Details" } // Parameter defaults
            );

            routes.MapRoute(
                "DetailsMobile", // Route name
                "letter/{id}/mobile", // URL with parameters
                new { controller = "Home", action = "Details", mobile = 1 } // Parameter defaults
            );


            routes.MapRoute(
                "Feedback", // Route name
                "feedback/", // URL with parameters
                new { controller = "Home", action = "Contact" } // Parameter defaults
            );

            routes.MapRoute(
                "Error", // Route name
                "error/", // URL with parameters
                new { controller = "Error", action = "HttpError" } // Parameter defaults
            );

            routes.MapRoute(
                "Best", // Route name
                "best/", // URL with parameters
                new { controller = "Home", action = "Best" } // Parameter defaults
            );

            routes.MapRoute(
                "PasswordStarterRoute", // Route name
                "password/", // URL with parameters
                new { controller = "Home", action = "Password"} // Parameter defaults
            );

            routes.MapRoute(
                "PasswordRoute", // Route name
                "password/{id}", // URL with parameters
                new { controller = "Home", action = "Password" } // Parameter defaults
            );


            routes.MapRoute(
                "BookmarksRoute", // Route name
                "bookmarks/", // URL with parameters
                new { controller = "Account", action = "Index" } // Parameter defaults
            );

            routes.MapRoute(
                "AddBookmarkRoute", // Route name
                "bookmark/{id}", // URL with parameters
                new { controller = "Account", action = "Bookmark" } // Parameter defaults
            );

            routes.MapRoute(
                "SongsRoute", // Route name
                "songs/", // URL with parameters
                new { controller = "Song", action = "Index" } // Parameter defaults
            );

            routes.MapRoute(
                "RSSRoute", // Route name
                "rss/", // URL with parameters
                new { controller = "Home", action = "RSS" } // Parameter defaults
            );


            routes.MapRoute(
                "SongsAddRoute", // Route name
                "songs/add/", // URL with parameters
                new { controller = "Song", action = "Add" } // Parameter defaults
            );

            routes.MapRoute(
                "SongsPreviewRoute", // Route name
                "songs/preview/", // URL with parameters
                new { controller = "Song", action = "Add" } // Parameter defaults
            );


            routes.MapRoute(
                "loginRoute", // Route name
                "login/", // URL with parameters
                new { controller = "Account", action = "Login" } // Parameter defaults
            );

            routes.MapRoute(
                "logoutRoute", // Route name
                "logout/", // URL with parameters
                new { controller = "Account", action = "Logout" } // Parameter defaults
            );

            routes.MapRoute(
                "Default", // Route name
                "{controller}/{action}/{id}", // URL with parameters
                new { controller = "Home", action = "Index", id = UrlParameter.Optional } // Parameter defaults
            );

        }

        protected void Application_Start()
        {

            RouteTable.Routes.MapHubs();

            AreaRegistration.RegisterAllAreas();

            RegisterGlobalFilters(GlobalFilters.Filters);
            RegisterRoutes(RouteTable.Routes);

            // ignoring signalr items for now
            var ignored = MiniProfiler.Settings.IgnoredPaths.ToList();
            ignored.Add("/signalr/");
            ignored.Add("/signalr");

            MiniProfiler.Settings.IgnoredPaths = ignored.ToArray();
        }

        protected void Application_BeginRequest()
        {

          if (Request.IsLocal)
          {
            MiniProfiler.Start();
          } 

        }

        protected void Application_AuthenticateRequest(Object sender, EventArgs e)
        {

          //if (User != null)
          //{

          //  //System.Diagnostics.Debug.Print("User: " + User.Identity.Name.ToString());
          //  if (User.Identity.Name != "seth")
          //  {
          //    MvcMiniProfiler.MiniProfiler.Stop(discardResults: true);
          //  }

          //}
          //else
          //{
          //  //System.Diagnostics.Debug.Print("Null user. Disable profiling.");
          //  //MvcMiniProfiler.MiniProfiler.Stop(discardResults: true);
          //}

        }

        protected void Application_Error(object sender, EventArgs e)
        {

            Exception ex = Server.GetLastError();
            Application[HttpContext.Current.Request.UserHostAddress.ToString()] = ex;
            System.Diagnostics.Debug.Print(ex.Message.ToString());

        }
    

        protected void Application_EndRequest()
        {

          MiniProfiler.Stop();

        }

    }
}