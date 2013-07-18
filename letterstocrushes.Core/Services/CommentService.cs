using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using letterstocrushes.Core.Interfaces;
using letterstocrushes.Core.Model;

namespace letterstocrushes.Core.Services
{
    public class CommentService
    {

        private readonly IQueryLetters _queryLetters;
        private readonly IQueryComments _queryComments;
        private readonly MailService _mailService;
        public CommentService(IQueryLetters queryLetters, IQueryComments queryComments, MailService mailService)
        {
            _queryLetters = queryLetters;
            _queryComments = queryComments;
            _mailService = mailService;
        }

        public List<Comment> getComments(int id, Boolean include_hidden)
        {
            return _queryComments.getComments(id, include_hidden);
        }

        public void AddComment(Comment comment, string host)
        {
            Letter lucky_letter = _queryLetters.getLetter(comment.letterId);

            // time to ban people
            List<String> banned_commenters = new List<String>();
            banned_commenters.Add("10315e2a-e671-4c1c-91be-4aff797bf852");
            banned_commenters.Add("33ce0d70-cf48-49c9-9ab2-0fe61ebc84f8");
            banned_commenters.Add("82f7c276-bdee-4e09-943e-39b45f3e7a07");
            //banned_commenters.Add("26ace590-d40e-4a03-a968-d160744437f5");

            if (banned_commenters.Contains(comment.commenterGuid))
            {
                _mailService.SendContact("Banned comment: <br><br>" + comment.commenterName + " (" + comment.commenterGuid + "): " + comment.commentMessage, "seth.hayward@gmail.com");
                return;
            }

            //
            // sanitize the input
            //

            // 
            // add some basic html to the comment to make it look better 
            // 

            string basic_text = comment.commentMessage;

            // first, we make sure that the first line
            // is a paragraph
            basic_text = "<p>" + basic_text;

            // then we make sure the last line closes it
            basic_text = basic_text + "</p>";

            // now all line breaks in the middle should
            // start new paragraphs
            basic_text = basic_text.Replace("\n", "</p><p>");

            comment.commentMessage = basic_text;

            if (comment.commenterName.Length == 0)
            {
                comment.commenterName = "anonymous lover";
            }

            // Send notifications before the latest comment is added.
            // this means the newest commenter does not get a notification,
            // which is what we want -- they already know they
            // have added a comment.
            SendNotifications(comment.letterId, host);

            _queryComments.AddComment(comment, lucky_letter);
        }

        public void SendNotifications(int letter_id, string host)
        {

            List<Comment> comments = getComments(letter_id, true);
            List<string> sent_address = new List<string>();

            foreach (Comment comment in comments)
            {

                System.Diagnostics.Debug.Print("Comment from " + comment.commenterEmail + " - wants update: " + comment.sendEmail);

                if (comment.sendEmail == true && sent_address.Contains(comment.commenterEmail) == false)
                {

                    Contact notification = new Contact();
                    notification.Email = comment.commenterEmail;
                    notification.Message = "Hello, <br><br>"
                        + "A new comment has been posted to " + host + "letter/" + comment.letterId
                        + "<br><br>Thought you'd like to know,<br>the letters to crushes team<br><br>_____________<br>unsubscribe: " + host + "unsubscribe/" + comment.letterId + "/" + comment.commenterEmail;

                    // add the email to a list of previously sent addresses
                    // we do not want to send multiple email notifications
                    // if they have commented in the same thread.
                    sent_address.Add(comment.commenterEmail);

                    _mailService.SendCommentNotification(notification.Message, notification.Email);

                }

            }

        }

        public Letter Unsubscribe(string email, int letter_id)
        {
            Boolean result = false;

            Letter unsubscribe = _queryLetters.getLetter(letter_id);

            if (unsubscribe != null)
            {
                result = _queryComments.Unsubscribe(email, letter_id);
            }

            return unsubscribe;
        }

        public List<Comment> getRecentComments(int page)
        {
            return _queryComments.getRecentComments(page);
        }

        public int getCommentCount()
        {
            return _queryComments.getCommentCount();
        }

        public Comment getComment(int id)
        {
            return _queryComments.getComment(id);
        }

        public Boolean hideComment(int id, string commenter_guid, string user_name, bool is_user_mod)
        {

            // only allow editing if the user is a mod,
            // OR, their guid matches the guid on the comment

            Comment comment = _queryComments.getComment(id);
            if (commenter_guid == comment.commenterGuid || is_user_mod == true)
            {
                return _queryComments.hideComment(id, commenter_guid, user_name);
            }
            else
            {
                return false;
            }
            
        }

        public Boolean EditComment(string commentText, int id, string commenter_guid, string user_name, bool is_user_mod)
        {
            // che
            return _queryComments.editComment(commentText, id, commenter_guid, user_name);
        }

    }
}
