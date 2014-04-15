using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using letterstocrushes.Core.Interfaces;
using letterstocrushes.Core.Model;

namespace letterstocrushes.Core.Services
{

    public class LetterService
    {

        private readonly BlockService _blockService;
        private readonly BookmarkService _bookmarkService;
        private readonly MailService _mailService;
        private readonly IQueryLetters _queryLetters;

        public LetterService(IQueryLetters queryLetters, MailService mailService, BookmarkService bookmarkService, BlockService blockService)
        {
            _queryLetters = queryLetters;
            _mailService = mailService;
            _bookmarkService = bookmarkService;
            _blockService = blockService;
        }

        public int getLetterCount()
        {
            return _queryLetters.getLetterCount();
        }

        public int getLetterCountMorePage()
        {
            return _queryLetters.getLetterCountMorePage();
        }

        public int getLetterCountHomePage()
        {
            return _queryLetters.getLetterCountHomePage();
        }

        public int getLetterCountModPage()
        {
            return _queryLetters.getLetterCountModPage();
        }

        public Letter getLetter(int id)
        {
            return _queryLetters.getLetter(id);
        }

        public Letter getLetterByTag(string guid)
        {
            return _queryLetters.getLetterByTag(guid);
        }


        public Letter getLastLetterSent()
        {
            return _queryLetters.getLastLetterSent();
        }

        public Letter getLastLetterFromIp(string ip)
        {
            return _queryLetters.getLastLetterFromIP(ip);
        }

        public List<Letter> getLastHoursLettersFromIp(string ip)
        {
            return _queryLetters.getLastHoursLettersFromIP(ip);
        }

        public void AddLetter(Letter letter)
        {
            _queryLetters.AddLetter(letter);
        }

        public Letter Mail(string letterText, string letterCountry, string user_ip, string user_id, int mobile, ref string error_message)
        {

            Letter new_letter = new Letter();

            HtmlUtility utility = HtmlUtility.Instance;

            try
            {

                Letter last_letter = new Letter();
                last_letter = getLastLetterSent();

                // why isn't this automatically generated anymore?
                new_letter.Id = last_letter.Id + 1;

                new_letter.letterLevel = 0;

                // PREPARE FOR A XSS ATTACK
                new_letter.letterMessage = utility.SanitizeHtml(letterText);

                new_letter.letterMessage = new_letter.letterMessage.Replace("text-decoration%3a%20line-through%3b", "text-decoration: line-through");
                new_letter.letterMessage = new_letter.letterMessage.Replace("text-decoration%3a%20underline%3b", "text-decoration: underline;");

                // if this is a mobile submission, we want to make it look
                // basic with some html
                // output goal:
                // <p>{para1}
                //  </p>
                //  <p>{para2}
                //  </p>
                //


                // if mobile is set to this, we want to set the
                // letter level to be a hidden level
                if (mobile == -10)
                {
                    new_letter.letterLevel = -10;
                }

                if (mobile == 1)
                {

                    string basic_text = new_letter.letterMessage;

                    // first, we make sure that the first line
                    // is a paragraph
                    basic_text = "<p>" + basic_text;

                    // then we make sure the last line closes it
                    basic_text = basic_text + "</p>";

                    // now all line breaks in the middle should
                    // start new paragraphs
                    basic_text = basic_text.Replace("\n", "</p><p>");

                    new_letter.letterMessage = basic_text;

                }

                new_letter.letterUp = 1;
                new_letter.senderCountry = letterCountry;
                new_letter.letterPostDate = DateTime.UtcNow;
                new_letter.letterLanguage = "en-US";
                new_letter.letterComments = 0;

                string _guid;
                _guid = System.Guid.NewGuid().ToString();
                new_letter.letterTags = _guid;

                new_letter.senderIP = user_ip;

                //
                // spam protection, december 2013
                //

                List<Letter> last_hour_of_letters_from_ip = getLastHoursLettersFromIp(user_ip);

                if (last_hour_of_letters_from_ip.Count > 3)
                {

                    Letter first_letter = last_hour_of_letters_from_ip.First();
                    TimeSpan wait = DateTime.UtcNow.Subtract(first_letter.letterPostDate);

                    string message = "429: Please only post 3 letters an hour. In the past hour you sent " + last_hour_of_letters_from_ip.Count.ToString() + " letters. You'll need to wait another ";
                    message += (60 - wait.Minutes).ToString() + " minutes before you can send again.";
                    
                    throw new Exception(message);
                }

                List<Block> subnet_blocks = _blockService.getBlocks(blockType.blockSubnet, blockWhat.blockLetter);
                // now... we need to search through each blocked subnet,
                // check to see if that is part of the problem...
                // -- if yes, we block
                // -- if no, continue to next

                foreach (Block subnet in subnet_blocks)
                {

                    if(user_ip.Contains(subnet.Value)) {
                        new_letter.letterLevel = -1;

                        Contact msg = new Contact();
                        msg.Email = "blocked " + user_ip;
                        msg.Message = string.Format("blocked (" + user_ip + ") due to subnet " + subnet.Value + ": \n{0}", new_letter.letterMessage.ToString());
                        msg.Message = msg.Message + "\nmobile: " + mobile;
                        _mailService.SendContact(msg.Message, msg.Email);

                        break;
                    };

                }


                List<Block> blocked_ips = _blockService.getBlocks(blockType.blockIP, blockWhat.blockLetter);

                List<string> banned_ips = new List<string>();

                foreach (Block b in blocked_ips)
                {
                    banned_ips.Add(b.Value);
                }

                banned_ips.Add("74.173.105.111");
                banned_ips.Add("76.109.29.135");
                banned_ips.Add("98.91.17.241");
                banned_ips.Add("72.200.107.25");
                banned_ips.Add("24.17.25.227");
                banned_ips.Add("75.168.39.103");
                banned_ips.Add("71.96.73.210");
                banned_ips.Add("24.14.242.118");
                banned_ips.Add("49.144.202.205");
                banned_ips.Add("125.60.241.47");
                banned_ips.Add("111.235.88.209");
                banned_ips.Add("66.168.196.58");
                banned_ips.Add("74.101.158.189");
                banned_ips.Add("190.235.4.27");
                banned_ips.Add("108.203.48.88");
                //banned_ips.Add("24.5.135.227"); // seth
                // banned_ips.Add("74.101.102.68"); this person seems okay now

                List<string> daily_limit_ips = new List<string>();
                daily_limit_ips.Add("112.209.63.245");  // diana
                daily_limit_ips.Add("112.209.42.203");  // diana
                daily_limit_ips.Add("190.235.111.7"); // may-01-2013, lots of one liners
                daily_limit_ips.Add("190.236.182.203"); // same type of letters
                daily_limit_ips.Add("108.249.83.97");   // seth test ip
                daily_limit_ips.Add("200.121.160.235");   // seth test ip

                if (daily_limit_ips.Contains(user_ip) == true)
                {

                    // these users only get to post one letter a day
                    Letter last_letter_from_this_ip = getLastLetterFromIp(user_ip);

                    if (last_letter_from_this_ip != null)
                    {

                        TimeSpan ban_time = new_letter.letterPostDate - last_letter_from_this_ip.letterPostDate;

                        if (ban_time.TotalHours <= 24)
                        {
                            new_letter.letterLevel = -1;

                            Contact msg = new Contact();
                            msg.Email = "spam ip blocked " + user_ip;
                            msg.Message = string.Format("spam ip blocked: \n{0}", new_letter.letterMessage.ToString());
                            _mailService.SendContact(msg.Message, msg.Email);
                        }

                    }

                }


                if (banned_ips.Contains(user_ip) == true)
                {

                    new_letter.letterLevel = -1;

                    Contact msg = new Contact();
                    msg.Email = "blocked " + user_ip;
                    msg.Message = string.Format("blocked (" + user_ip + "): \n{0}", new_letter.letterMessage.ToString());
                    msg.Message = msg.Message + "\nmobile: " + mobile;
                    _mailService.SendContact(msg.Message, msg.Email);

                }

                AddLetter(new_letter);

                if (new_letter.letterMessage.Contains("http"))
                {
                    Contact msg = new Contact();
                    msg.Email = "blocked " + user_ip;
                    msg.Message = string.Format("alert: \n{0}", new_letter.letterMessage.ToString() + "\n\nletter: http://www.letterstocrushes.com/letter/" + new_letter.Id.ToString());
                    _mailService.SendContact(msg.Message, msg.Email);
                }

                if (user_id != null)
                {
                    _bookmarkService.Add(new_letter.Id, user_id);
                }

            }
            catch (Exception ex)
            {

                Contact msg = new Contact();
                msg.Email = "seth.hayward@gmail.com";
                msg.Message = "Sending exception: <br/><br/>";
                msg.Message = msg.Message + "<br/>letterText: " + letterText;
                msg.Message = msg.Message + "<br/>letterCountry: " + letterCountry;                
                msg.Message = msg.Message + "<br/><br/>" + ex.Message.ToString();

                if (ex.InnerException != null)
                {
                    msg.Message = msg.Message + "<br/><br/>Inner Exception: <br/>:" + ex.InnerException.Message;
                }

                _mailService.SendContact(msg.Message, msg.Email);
                error_message = ex.Message.ToString();
                new_letter = null;
            }

            return new_letter;


        }

        public Letter getLetter(int id, string time_zone)
        {
            Letter letter = _queryLetters.getLetter(id);

            if (letter != null & time_zone != null)
            {

                letter = letter.Clone();

                int offset = 0;

                if (int.TryParse(time_zone, out offset) == false)
                {
                    offset = 0;
                }

                DateTime postdate = letter.letterPostDate;
                postdate = postdate.AddHours(offset * -1);
                letter.letterPostDate = postdate;
            }

            return letter;

        }

        public Letter getLatestFrontPageLetter()
        {
            return _queryLetters.getLatestFrontPageLetter();
        }

        public List<Letter> search(string terms)
        {            
            return _queryLetters.search(terms);
        }

        public List<Letter> searchByDate(string terms, int year, int month, int day, int time_zone)
        {
            return _queryLetters.searchDate(terms, year, month, day, time_zone);
        }

        public List<Letter> getLetters(int greater_than_level, int page, int _pagesize)
        {
            return _queryLetters.getLetters(greater_than_level, page, _pagesize);
        }

        public List<Letter> getModLetters(int page, int _pagesize)
        {
            return _queryLetters.getModLetters(page, _pagesize);
        }

        public List<Letter> getPopularLetters(Letter latest_front_page)
        {
            return _queryLetters.getPopularLetters(latest_front_page);
        }
        public Boolean hideLetter(int lucky_id, string userip, string cookie_value, string user_name, bool is_user_mod)
        {

            Boolean can_edit = this.canEdit(cookie_value, is_user_mod);

            if (can_edit == false)
            {
                return false;
            }
            else
            {
                _queryLetters.hideLetter(lucky_id, userip, cookie_value, user_name, is_user_mod);
                return true;
            }

        }

        public Boolean unhideLetter(int lucky_id, string userip, string cookie_value, string user_name, bool is_user_mod)
        {

            Boolean can_edit = this.canEdit(cookie_value, is_user_mod);

            if (can_edit == false)
            {
                return false;
            }
            else
            {
                _queryLetters.unhideLetter(lucky_id, userip, cookie_value, user_name, is_user_mod);
                return true;
            }

        }

        public Boolean facebookLetter(int lucky_id, long toFacebookUID, long fromFacebookUID, string userip, string cookie_value, string user_name, bool is_user_mod)
        {

            Boolean can_edit = this.canEdit(cookie_value, is_user_mod);

            if (can_edit == false)
            {
                return false;
            }
            else
            {
                _queryLetters.facebookLetter(lucky_id, toFacebookUID, fromFacebookUID, userip, cookie_value, user_name, is_user_mod);
                return true;
            }

        }


        public Boolean canEdit(string cookie_value, bool user_is_mod)
        {
            Boolean able_to_edit;

            // only if the cookie is there do we
            // know if they are able to edit it or not.
            // obviously, this could be a better system...

            if (cookie_value != null)
            {
                able_to_edit = true;
            }
            else
            {
                able_to_edit = false;
            }

            if (user_is_mod == true)
            {
                able_to_edit = true;
            }

            return able_to_edit;
        }

        public List<Letter> fixList(List<Letter> letters, string time_zone)
        {

            List<Letter> result = new List<Letter>();

            if (time_zone == string.Empty) { time_zone = "0"; }

            if (time_zone != null)
            {

                int offset = 0;
                offset = Convert.ToInt32(time_zone);

                foreach (Letter letter in letters)
                {
                    Letter temp_letter = letter.Clone();
                    DateTime postdate = temp_letter.letterPostDate;
                    postdate = postdate.AddHours(offset * -1);
                    temp_letter.letterPostDate = postdate;
                    result.Add(temp_letter);
                }

            }

            return result;

        }

        public Boolean editLetter(int letter_id, string new_letter, string userip, string cookie_value, string user_name, bool is_user_mod, int mobile = 0)
        {

            Letter lucky = _queryLetters.getLetter(letter_id);

            Boolean edited = false;

            if (lucky == null)
            {
                return false;
            }

            Boolean can_edit = this.canEdit(cookie_value, is_user_mod);

            if (can_edit == false)
            {
                return false;
            }

            if (mobile == 1)
            {

                string basic_text = new_letter;

                // first, we make sure that the first line
                // is a paragraph
                basic_text = "<p>" + basic_text;

                // then we make sure the last line closes it
                basic_text = basic_text + "</p>";

                // now all line breaks in the middle should
                // start new paragraphs
                basic_text = basic_text.Replace("\n", "</p><p>");

                new_letter = basic_text;

            }

            edited = _queryLetters.editLetter(letter_id, new_letter, userip, cookie_value, user_name, is_user_mod);

            return edited;
        }

    }
}
