using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using letterstocrushes.Core.Interfaces;
using AutoMapper;
using letterstocrushes.Core.Model;

namespace letterstocrushes.Infrastructure.Data
{
    public class EfQueryLetters : IQueryLetters
    {

        public EfQueryLetters()
        {
            Mapper.CreateMap<letter, Letter>();
            Mapper.CreateMap<Letter, letter>();
        }

        public void hideLetter(int lucky_id, string userip, string cookie_value, string user_name, bool is_user_mod)
        {

            db_mysql db_mysql = new db_mysql();
            db_mssql db_mssql = new db_mssql();

            Letter lucky = getLetter(lucky_id);

            if (lucky == null) { return; }

            edit new_edit = new edit();
            new_edit.editComment = "Hidden by " + user_name;
            new_edit.editor = user_name;

            new_edit.newLetter = lucky.letterMessage;
            new_edit.previousLetter = lucky.letterMessage;
            new_edit.status = "accepted";
            new_edit.letterID = lucky.Id;
            new_edit.editDate = DateTime.UtcNow;

            db_mssql.edits.Add(new_edit);

            letter transformed;            
            transformed = Mapper.Map<Letter, letter>(lucky);

            db_mysql.letters.Attach(transformed);
            var letter = db_mysql.Entry(transformed);

            letter.Property(e => e.letterLevel).IsModified = true;
            transformed.letterLevel = -1;

            letter.CurrentValues.SetValues(transformed);

            db_mysql.SaveChanges();
        }

        public void unhideLetter(int lucky_id, string userip, string cookie_value, string user_name, bool is_user_mod)
        {

            db_mysql db_mysql = new db_mysql();
            db_mssql db_mssql = new db_mssql();

            Letter lucky = getLetter(lucky_id);

            if (lucky == null) { return; }

            edit new_edit = new edit();
            new_edit.editComment = "Unhidden by " + user_name;
            new_edit.editor = user_name;

            new_edit.newLetter = lucky.letterMessage;
            new_edit.previousLetter = lucky.letterMessage;
            new_edit.status = "accepted";
            new_edit.letterID = lucky.Id;
            new_edit.editDate = DateTime.UtcNow;

            db_mssql.edits.Add(new_edit);

            letter transformed;
            transformed = Mapper.Map<Letter, letter>(lucky);

            db_mysql.letters.Attach(transformed);
            var letter = db_mysql.Entry(transformed);

            letter.Property(e => e.letterLevel).IsModified = true;
            transformed.letterLevel = 0;

            letter.CurrentValues.SetValues(transformed);

            db_mysql.SaveChanges();
        }

        public bool editLetter(int letter_id, string new_letter, string userip, string cookie_value, string user_name, bool is_user_mod)
        {

            db_mysql db_mysql = new db_mysql();
            db_mssql db_mssql = new db_mssql();

            Letter lucky = getLetter(letter_id);
            letter lucky_letter = Mapper.Map<Letter, letter>(lucky);

            HtmlUtility utility = HtmlUtility.Instance;

            edit new_edit = new edit();

            new_edit.editComment = "Edited by " + user_name;
            new_edit.editor = user_name;

            new_edit.editDate = DateTime.Now;

            String letter_contents = utility.SanitizeHtml(new_letter);
            letter_contents = letter_contents.Replace("text-decoration%3a%20line-through%3b", "text-decoration: line-through");
            letter_contents = letter_contents.Replace("text-decoration%3a%20underline%3b", "text-decoration: underline;");

            new_edit.newLetter = letter_contents;

            new_edit.previousLetter = lucky.letterMessage;
            new_edit.status = "accepted";
            new_edit.letterID = lucky.Id;
            new_edit.editDate = DateTime.UtcNow;

            db_mssql.edits.Add(new_edit);

            db_mysql.letters.Attach(lucky_letter);
            var letter = db_mysql.Entry(lucky_letter);

            letter.Property(e => e.letterMessage).IsModified = true;
            lucky_letter.letterMessage = letter_contents;

            db_mysql.SaveChanges();

            db_mssql.SaveChanges();

            return true;

        }

        public Letter getLetter(int id)
        {
            db_mysql db_mysql = new db_mysql();
            letter letter = (from m in db_mysql.letters where m.Id.Equals(id) select m).FirstOrDefault();
            return Mapper.Map<letter, Letter>(letter);
        }

        public Letter getLastLetterSent()
        {
            db_mysql db_mysql = new db_mysql();
            letter letter = (from m in db_mysql.letters orderby m.Id descending select m).First();
            return Mapper.Map<letter, Letter>(letter);
        }

        public int getLetterCount()
        {
            db_mysql db_mysql = new db_mysql();
            return db_mysql.letters.Count();
        }

        public int getLetterCountHomePage()
        {
            db_mysql db_mysql = new db_mysql();
            return (from m in db_mysql.letters where m.letterLevel > 0 select m).Count();
        }

        public int getLetterCountMorePage()
        {
            db_mysql db_mysql = new db_mysql();
            return (from m in db_mysql.letters where m.letterLevel >= 0 select m).Count();
        }

        public List<Letter> getLetters(int greater_than_level, int page, int _pagesize)
        {
            db_mysql db_mysql = new db_mysql();

            List<letter> results = new List<letter>();

            letter last = (from m in db_mysql.letters orderby m.Id descending select m).First();
            int greater_than_amount = 0;

            switch (greater_than_level)
            {
                case -1:
                    // assume that we can get our letters
                    // in a maximum of (10+5)*(1)= 12 letters
                    // for the first page... assumes 5 are hidden
                    // per page of 10.

                    // if we don't do this, it's mad slow...
                    greater_than_amount = last.Id - ((_pagesize + 5) * page);
                    break;
                case 0:
                    greater_than_amount = 0;
                    break;
            }

            var query = (from m in db_mysql.letters
                         where m.letterLevel > greater_than_level
                             && m.Id > greater_than_amount
                         orderby m.Id descending
                         select m);

            results = query.Skip((page - 1) * _pagesize).Take(_pagesize).ToList();
            return Mapper.Map<List<letter>, List<Letter>>(results);
        }

        public Core.Model.Letter getLatestFrontPageLetter()
        {
            db_mysql db_mysql = new db_mysql();
            letter letter = (from l in db_mysql.letters where l.letterLevel == 1 orderby l.letterPostDate descending select l).First();

            return Mapper.Map<letter, Letter>(letter);
        }

        public List<Core.Model.Letter> getPopularLetters(Core.Model.Letter latest_front_page)
        {

            db_mysql db_mysql = new db_mysql();
            db_mssql db_mssql = new db_mssql();

            List<letter> results = new List<letter>();

            List<TopListData_Result> top_votes = new List<TopListData_Result>();
            top_votes = (from l in db_mssql.TopListData() where l.hearts > 5 select l).ToList();

            List<long> top_votes_id = new List<long>();
            top_votes_id = (from l in top_votes select l.letterID).ToList();

            results = (from z in db_mysql.letters where top_votes_id.Contains(z.Id) && z.letterLevel == 0 && z.letterPostDate > latest_front_page.letterPostDate orderby z.letterPostDate ascending select z).Take(500).ToList();

            return Mapper.Map<List<letter>, List<Letter>>(results);

        }

        public List<Core.Model.Letter> search(string terms)
        {
            db_mysql db_mysql = new db_mysql();

            List<letter> results = new List<letter>();
            results = db_mysql.quickSearch(terms).ToList();

            return Mapper.Map<List<letter>, List<Letter>>(results);
        }


        public Letter getLastLetterFromIP(string ip)
        {
            db_mysql db_mysql = new db_mysql();
            letter letter = (from m in db_mysql.letters where m.senderIP.Contains(ip) && m.letterLevel == 0 orderby m.letterPostDate descending select m).FirstOrDefault();
            return Mapper.Map<letter, Letter>(letter);
        }


        public void AddLetter(Letter letter)
        {
            db_mysql db_mysql = new db_mysql();
            letter new_letter = Mapper.Map<Letter, letter>(letter);
            db_mysql.letters.Add(new_letter);
            db_mysql.SaveChanges();
        }


        public Letter getLetterByTag(string guid)
        {
            db_mysql db_mysql = new db_mysql();
            letter letter = (from m in db_mysql.letters where m.letterTags.Equals(guid) select m).FirstOrDefault();
            return Mapper.Map<letter, Letter>(letter);
        }


        public void facebookLetter(int lucky_id, long toFacebookUID, long fromFacebookUID, string userip, string cookie_value, string user_name, bool is_user_mod)
        {

            db_mysql db_mysql = new db_mysql();

            Letter lucky = getLetter(lucky_id);

            if (lucky == null) { return; }

            letter transformed;
            transformed = Mapper.Map<Letter, letter>(lucky);

            db_mysql.letters.Attach(transformed);
            var letter = db_mysql.Entry(transformed);

            letter.Property(e => e.fromFacebookUID).IsModified = true;
            transformed.fromFacebookUID = fromFacebookUID;

            letter.Property(e => e.toFacebookUID).IsModified = true;
            transformed.toFacebookUID = toFacebookUID;

            letter.CurrentValues.SetValues(transformed);

            db_mysql.SaveChanges();

        }
    }
}
