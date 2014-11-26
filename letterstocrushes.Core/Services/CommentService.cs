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
        private readonly BlockService _blockService;
        public CommentService(IQueryLetters queryLetters, IQueryComments queryComments, MailService mailService, BlockService blockService)
        {
            _queryLetters = queryLetters;
            _queryComments = queryComments;
            _mailService = mailService;
            _blockService = blockService;
        }

        public List<Comment> getComments(int id, Boolean include_hidden)
        {
            return _queryComments.getComments(id, include_hidden);
        }

        public void AddComment(Comment comment, string host)        
        {

            // spam protection
            List<Block> blocked_ips = _blockService.getBlocks(blockType.blockIP, blockWhat.blockComment);

            List<string> banned_ips = new List<string>();

            foreach (Block b in blocked_ips)
            {
                banned_ips.Add(b.Value);
            }


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


            banned_ips.Add("100.2.225.62");

            if(banned_ips.Contains(comment.commenterIP)) {
                //_mailService.SendContact("Banned comment (ip, " + comment.commenterIP + "): <br><br>" + comment.commenterName + " (" + comment.commenterGuid + "): " + comment.commentMessage, "seth.hayward@gmail.com"); 
                return;
            }

            // if an ip starts with any one of these,
            // we're going to block these lamers.
            List<String> spammer_ips = new List<String>();
            spammer_ips.Add("65.49.14");
            spammer_ips.Add("111.118.37");
            spammer_ips.Add("119.226.253");
            spammer_ips.Add("79.123.220");
            spammer_ips.Add("58.22.10");
            spammer_ips.Add("218.108.85");
            spammer_ips.Add("202.121.96");
            spammer_ips.Add("110.170.46");
            spammer_ips.Add("219.141.240");
            spammer_ips.Add("46.105.114");
            spammer_ips.Add("124.158.1");
            spammer_ips.Add("137.175.118");
            spammer_ips.Add("93.115.94");
            spammer_ips.Add("213.175.167");

            List<Block> blocked_subnet_ips = _blockService.getBlocks(blockType.blockSubnet, blockWhat.blockComment);

            List<string> ban_list_v2 = new List<String>();

            foreach (Block b in blocked_subnet_ips)
            {
                ban_list_v2.Add(b.Value);
            }

            if(comment.commenterIP != null && spammer_ips.Any(rax=>comment.commenterIP.StartsWith(rax))) {
                //_mailService.SendContact("Spammer shut down, ip: " + comment.commenterIP, "seth.hayward@gmail.com");
                return;
            }

            if (comment.commenterIP != null && ban_list_v2.Any(rax => comment.commenterIP.StartsWith(rax)))
            {
                string subnet_ip = ban_list_v2.Any(rax => comment.commenterIP.StartsWith(rax)).ToString();
                string blockedMsg = String.Format("blocked comment due to subnet (subnet: {0}, ip: {1}): <br /><br />{2}", subnet_ip, comment.commenterIP, comment.commentMessage);
                _mailService.SendContact(blockedMsg, "seth.hayward@gmail.com");
                return;
            }


            if(comment.commentMessage != null && comment.commentMessage.Contains("mygardeningplace.com")) {
                _mailService.SendContact("mygardeningplace spam shut down.", "seth.hayward@gmail.com");
                return;
            }

            if (comment.commentMessage != null && comment.commentMessage.Contains("mycraftingplace.com"))
            {
                _mailService.SendContact("mycraftingplace.com spam shut down.", "seth.hayward@gmail.com");
                return;
            }

            if (comment.commentMessage != null && comment.commentMessage.Contains("bio2008.org"))
            {
                _mailService.SendContact("bio2008.org spam shut down.", "seth.hayward@gmail.com");
                return;
            }

            if (comment.commentMessage != null && comment.commentMessage.Contains("countylinechiro.com"))
            {
                _mailService.SendContact("bio2008.org spam shut down.", "seth.hayward@gmail.com");
                return;
            }

            if (comment.commentMessage != null && comment.commentMessage.Contains("cfnmtoob.com"))
            {
                _mailService.SendContact("cfnmtoob.com spam shut down.", "seth.hayward@gmail.com");
                return;
            }

            //if (comment.commentMessage != null && comment.commentMessage.Contains("http://"))
            //{
            //    //_mailService.SendContact("link shut down: <br/>" + comment.commentMessage, "seth.hayward@gmail.com");
            //    return;
            //}

            // /connect.masslive.com

            if (comment.commentMessage != null && comment.commentMessage.Contains("connect.masslive.com"))
            {
                //_mailService.SendContact("connect.masslive.com spam shut down.", "seth.hayward@gmail.com");
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

        public Boolean unhideComment(int id, string commenter_guid, string user_name, bool is_user_mod)
        {

            // only allow editing if the user is a mod,
            // OR, their guid matches the guid on the comment

            Comment comment = _queryComments.getComment(id);
            if (commenter_guid == comment.commenterGuid || is_user_mod == true)
            {
                return _queryComments.unhideComment(id, commenter_guid, user_name);
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
