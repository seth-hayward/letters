using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.IO.Compression;
using System.Diagnostics;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;

namespace letterstocrushes
{

    public class Helpers
    {

        public static string CommentTitle(double comments)
        {
            string result = "0 comments";

            if (comments == 0)
            {
                result = "0 comments";
            }
            else if (comments == 1)
            {
                result = "1 comment";
            }
            else if (comments > 1)
            {
                result = comments.ToString() + " comments";
            }

            return result;

        }

        public static string TimeAgo(DateTime when)
        {

            var ts = new TimeSpan(DateTime.UtcNow.Ticks - when.Ticks);
            double delta = Math.Abs(ts.TotalSeconds);

            if (delta < 60)
            {
                return ts.Seconds == 1 ? "one second ago" : ts.Seconds + " seconds ago";
            }
            if (delta < 120)
            {
                return "a minute ago";
            }
            if (delta < 2700) // 45 * 60
            {
                return ts.Minutes + " minutes ago";
            }
            if (delta < 5400) // 90 * 60
            {
                return "an hour ago";
            }
            if (delta < 86400) // 24 * 60 * 60
            {
                return ts.Hours + " hours ago";
            }
            if (delta < 172800) // 48 * 60 * 60
            {
                return "yesterday";
            }
            if (delta < 2592000) // 30 * 24 * 60 * 60
            {
                return ts.Days + " days ago";
            }
            if (delta < 31104000) // 12 * 30 * 24 * 60 * 60
            {
                int months = Convert.ToInt32(Math.Floor((double)ts.Days / 30));
                return months <= 1 ? "one month ago" : months + " months ago";
            }
            int years = Convert.ToInt32(Math.Floor((double)ts.Days / 365));
            return years <= 1 ? "one year ago" : years + " years ago";

        }

        public static string Pager(double current_page, double items, string root, bool mobile = false)
        {

            string response = "<div class='pager'>";
            double pages = Math.Ceiling(items / 10);

            // back button first
            if (current_page == 1)
            {
                response += Button("back", true, root, current_page - 1, false, mobile);
            }
            else
            {
                response += Button("back", false, root, current_page - 1, false, mobile);
            }

            // middle text

            //
            // the following two if statements are mutually exclusive,
            // only because we are always dealing with more than 5 pages
            //

            if (mobile == false)
            {

                if (pages > 4)
                {

                    if (current_page < 4)
                    {

                        for (int a = 1; a < 6; a++)
                        {
                            response += Button(a.ToString(), false, root, (double)a, a == current_page, mobile);
                        }

                    }

                    if (current_page >= 4 & current_page < (pages - 2))
                    {

                        for (int a = (int)(current_page - 2); a < (int)(current_page + 3); a++)
                        {
                            response += Button(a.ToString(), false, root, (double)a, a == current_page, mobile);
                        }

                    }

                    if (current_page >= (pages - 2))
                    {

                        for (int a = (int)(pages - 4); a <= (int)pages; a++)
                        {
                            response += Button(a.ToString(), false, root, (double)a, a == current_page, mobile);
                        }

                    }


                }


            }


            // next button

            //Debug.Print("Current page: " + current_page.ToString() + " of " + pages.ToString());

            if (current_page == pages)
            {
                response += Button("next", true, root, current_page + 1, false, mobile);
            }
            else
            {
                response += Button("next", false, root, current_page + 1, false, mobile);
            }


            response += "</div>";

            return response;
        }

        public static string SearchPager(double current_page, double items, string root, string terms)
        {

            if (current_page == 0) { current_page = 1; }

            string response = "<div class='pager'>";
            //double pages = Math.Round(items / 10, 0, MidpointRounding.AwayFromZero);
            double pages = Math.Ceiling(items / 10);

            // back button first
            if (current_page == 1)
            {
                response += SearchButton("back", true, root, current_page - 1, terms);
            }
            else
            {
                response += SearchButton("back", false, root, current_page - 1, terms);
            }

            if (current_page == pages)
            {
                // next button, enabled
                response += "&nbsp;" + SearchButton("next", true, root, current_page + 1, terms);
            }
            else
            {
                // next button, disabled
                response += "&nbsp;" + SearchButton("next", false, root, current_page + 1, terms);
            }

            response += "</div>";

            return response;
        }

        public static string ArchivePager(double current_page, double items, string root, string terms)
        {

            if (current_page == 0) { current_page = 1; }

            string response = "<div class='pager'>";
            //double pages = Math.Round(items / 10, 0, MidpointRounding.AwayFromZero);
            double pages = Math.Ceiling(items / 10);

            // back button first
            if (current_page == 1)
            {
                response += ArchiveButton("back", true, root, current_page - 1, terms);
            }
            else
            {
                response += ArchiveButton("back", false, root, current_page - 1, terms);
            }

            if (current_page == pages)
            {
                // next button, enabled
                response += "&nbsp;" + ArchiveButton("next", true, root, current_page + 1, terms);
            }
            else
            {
                // next button, disabled
                response += "&nbsp;" + ArchiveButton("next", false, root, current_page + 1, terms);
            }

            response += "</div>";

            return response;
        }

        public static string ArchiveButton(string text, bool disabled, string url, double page, string terms)
        {

            string response = "";

            if (disabled == true)
            {
                response = string.Format("<a class='disabled' onclick='$.post(this.href); return false;'>{0}</a>", text);
            }
            else
            {
                response = string.Format("<a class='back' href='{1}&page={2}'>{0}</a>", text, url, page, terms);
            }

            return response;

        }

        public static string SearchButton(string text, bool disabled, string url, double page, string terms)
        {

            string response = "";

            if (disabled == true)
            {
                response = string.Format("<a class='disabled' onclick='$.post(this.href); return false;'>{0}</a>", text);
            }
            else
            {
                response = string.Format("<a class='back' href='{1}?terms={3}&page={2}'>{0}</a>", text, url, page, terms);
            }

            return response;

        }

        public static string Button(string text, bool disabled, string url, double page, bool current = false, bool mobile = false)
        {

            string response = "";
            string mobile_include = "";

            if (mobile)
            {
                mobile_include = @"data-role=""button"" data-ajax=""false"" data-inline=""true""";
            }

            if (disabled == true) {
                response = string.Format("<a class='disabled' {1} onclick='$.post(this.href); return false;'>{0}</a>", text, mobile_include);
            } else {

                // if we have the current button (this is the page that the user is on)
                // we want to style the button a little differently.

                if (current == true)
                {
                    response = string.Format("<a class='current' {3} href='{1}page/{2}'>{0}</a>", text, url, page, mobile_include);
                }
                else
                {
                    response = string.Format("<a {3} href='{1}page/{2}'>{0}</a>", text, url, page, mobile_include);
                }

            }

            return response;

        }

        //
        // This class if from http://www.rohland.co.za/index.php/2009/10/31/csharp-html-diff-algorithm/
        //  
        // License:
        // Copyright (c) 2009 Nathan Herald, Rohland de Charmoy
        // Permission is hereby granted, free of charge, to any person obtaining a copy of this software and
        // associated documentation files (the "Software"), to deal in the Software without restriction,
        // including without limitation the rights to use, copy, modify, merge, publish, distribute, sublicense,
        // and/or sell copies of the Software, and to permit persons to whom the Software is furnished to do so,
        // subject to the following conditions:
        //
        //- The above copyright notice and this permission notice shall be included in all copies or substantial portions of the Software.
        //- THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES
        //  OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE
        //  LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN
        //  CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE. 

    }

    public class CompressFilter : ActionFilterAttribute
    {
        public override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            HttpRequestBase request = filterContext.HttpContext.Request;

            string acceptEncoding = request.Headers["Accept-Encoding"];

            if (string.IsNullOrEmpty(acceptEncoding)) return;

            acceptEncoding = acceptEncoding.ToUpperInvariant();

            HttpResponseBase response = filterContext.HttpContext.Response;

            if (acceptEncoding.Contains("GZIP"))
            {
                response.AppendHeader("Content-encoding", "gzip");
                response.Filter = new GZipStream(response.Filter,
                    CompressionMode.Compress);
            }
            else if (acceptEncoding.Contains("DEFLATE"))
            {
                response.AppendHeader("Content-encoding", "deflate");
                response.Filter = new DeflateStream(response.Filter,
                    CompressionMode.Compress);
            }
        }
    }

}