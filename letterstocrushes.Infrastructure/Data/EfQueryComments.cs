using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using letterstocrushes.Core.Interfaces;
using letterstocrushes.Core.Model;
using AutoMapper;

namespace letterstocrushes.Infrastructure.Data
{
    public class EfQueryComments : IQueryComments
    {

        public EfQueryComments()
        {
            Mapper.CreateMap<comment, Comment>();
            Mapper.CreateMap<Comment, comment>();
            Mapper.CreateMap<Letter, letter>();
        }

        public List<Comment> getComments(int id, Boolean include_hidden)
        {

            db_mysql db_mysql = new db_mysql();
            List<comment> comments = new List<comment>();

            if (include_hidden == true)
            {

                comments = (from m in db_mysql.comments where m.letterId.Equals(id) && m.level != -2 select m).ToList();

            }
            else
            {
                comments = (from m in db_mysql.comments where m.letterId.Equals(id) && m.level >= 0 select m).ToList();
            }

            return Mapper.Map<List<comment>, List<Comment>>(comments);

        }

        public void AddComment(Comment comment, Letter letter)
        {

            letter transposed = Mapper.Map<Letter, letter>(letter);

            db_mysql db_mysql = new db_mysql();
            db_mysql.letters.Attach(transposed);
            var letter_obj = db_mysql.Entry(transposed);


            if (comment.level > -1)
            {
                letter_obj.Property(e => e.letterComments).IsModified = true;
                transposed.letterComments = transposed.letterComments + 1;
            }

            db_mysql.comments.Add(Mapper.Map<Comment, comment>(comment));
            db_mysql.SaveChanges();

        }

        public Boolean Unsubscribe(string email, int letter_id)
        {

            Boolean result = false;

            db_mysql db_mysql = new db_mysql();
            List<comment> comments = (from m in db_mysql.comments where m.letterId.Equals(letter_id) && m.commenterEmail.Equals(email) select m).ToList();

            if (comments.Count > 0)
            {
                // update the settings so this user does not get emails
                // in the future

                foreach (comment cmt in comments)
                {
                    db_mysql.comments.Attach(cmt);
                    var letter_comment = db_mysql.Entry(cmt);

                    letter_comment.Property(e => e.sendEmail).IsModified = true;
                    cmt.sendEmail = false;
                    db_mysql.SaveChanges();
                }

                result = true;
            }

            return result;
        }

        public List<Comment> getRecentComments(int page)
        {
            db_mysql db_mysql = new db_mysql();
            var query = (from m in db_mysql.comments
                         where m.level.Value != -2
                         orderby m.Id descending
                         select m);

            // hard coding page size, is it worth the extra parameter? it never changes
            List<comment> results = query.Skip((page - 1) * 10).Take(10).ToList();
            return Mapper.Map<List<comment>, List<Comment>>(results);
        }

        public int getCommentCount()
        {
            db_mysql db_mysql = new db_mysql();
            return db_mysql.comments.Count();
        }

        public Comment getComment(int id)
        {
            db_mysql db_mysql = new db_mysql();
            comment selected_comment = (from m in db_mysql.comments where m.Id.Equals(id) select m).FirstOrDefault();
            return Mapper.Map<comment, Comment>(selected_comment);
        }


        public bool editComment(string commentText, int id, string commenter_guid, string user_name)
        {
            db_mysql db_mysql = new db_mysql();
            db_mssql db_mssql = new db_mssql();

            Comment lucky_comment = getComment(id);
            comment comment = Mapper.Map<Comment, comment>(lucky_comment);

            HtmlUtility utility = HtmlUtility.Instance;

            edit new_edit = new edit();

            new_edit.editComment = "Comment edited by " + user_name;
            new_edit.editor = user_name;

            new_edit.editDate = DateTime.Now;

            String comment_contents = utility.SanitizeHtml(commentText);

            new_edit.newLetter = comment_contents;

            new_edit.previousLetter = lucky_comment.commentMessage;
            new_edit.status = "accepted";
            new_edit.letterID = lucky_comment.letterId;
            new_edit.editDate = DateTime.UtcNow;

            db_mssql.edits.Add(new_edit);

            db_mysql.comments.Attach(comment);
            var comment_var = db_mysql.Entry(comment);

            comment_var.Property(e => e.commentMessage).IsModified = true;
            comment.commentMessage = comment_contents;

            db_mysql.SaveChanges();
            db_mssql.SaveChanges();

            return true;
        }


        public bool hideComment(int id, string commenter_guid, string user_name)
        {

            db_mysql db_mysql = new db_mysql();
            db_mssql db_mssql = new db_mssql();

            Comment lucky_comment = getComment(id);
            comment comment = Mapper.Map<Comment, comment>(lucky_comment);

            letter letter = (from m in db_mysql.letters where m.Id.Equals(lucky_comment.letterId) select m).FirstOrDefault();

            HtmlUtility utility = HtmlUtility.Instance;

            edit new_edit = new edit();

            new_edit.editComment = "Comment hidden by " + user_name;
            new_edit.editor = user_name;

            new_edit.editDate = DateTime.Now;

            new_edit.newLetter = lucky_comment.commentMessage;

            new_edit.previousLetter = lucky_comment.commentMessage;
            new_edit.status = "accepted";
            new_edit.letterID = lucky_comment.letterId;
            new_edit.editDate = DateTime.UtcNow;

            db_mssql.edits.Add(new_edit);

            db_mysql.comments.Attach(comment);
            db_mysql.letters.Attach(letter);

            var comment_var = db_mysql.Entry(comment);
            var letter_var = db_mysql.Entry(letter);

            comment_var.Property(e => e.level).IsModified = true;
            comment.level = -1;

            letter_var.Property(e => e.letterComments).IsModified = true;
            letter.letterComments = letter.letterComments - 1;

            db_mysql.SaveChanges();
            db_mssql.SaveChanges();

            return true;


        }
    }
}
