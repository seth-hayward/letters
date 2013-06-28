using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using letterstocrushes.Core.Model;
using letterstocrushes.Core.Interfaces;
using AutoMapper;

namespace letterstocrushes.Infrastructure.Data
{
    public class EfQueryQueue : IQueryQueue
    {

        public EfQueryQueue()
        {
            Mapper.CreateMap<letter, Letter>();
        }

        public List<Letter> getQueuedLetters()
        {
            db_mssql db_mssql = new db_mssql();
            db_mysql db_mysql = new db_mysql();

            List<Queued> qry = (from m in db_mssql.Queueds where m.State == 0 orderby m.LetterID ascending select m).ToList();
            List<letter> queued_letters = new List<letter>();

            foreach (Queued que in qry)
            {
                letter queued_letter = (from m in db_mysql.letters where m.Id == que.LetterID select m).FirstOrDefault();

                if (queued_letter != null)
                {
                    queued_letters.Add(queued_letter);
                }
            }

            return Mapper.Map<List<letter>, List<Letter>>(queued_letters);
        }

        public void Dequeue(int letter_id)
        {

            db_mssql db_mssql = new db_mssql();

            Queued deque = (from m in db_mssql.Queueds where m.LetterID == letter_id && m.State == 0 select m).FirstOrDefault();
            if (deque != null)
            {

                db_mssql.Queueds.Attach(deque);
                var entry = db_mssql.Entry(deque);
                entry.Property(e => e.State).IsModified = true;
                deque.State = 1;

                db_mssql.SaveChanges();
            }
        }

        public void Queue(int letter_id, string user_name)
        {

            db_mssql db_mssql = new db_mssql();
            db_mysql db_mysql = new db_mysql();

            letter lucky = (from m in db_mysql.letters where m.Id.Equals(letter_id) select m).FirstOrDefault();

            if (lucky != null)
            {

                edit new_edit = new edit();

                new_edit.editComment = "Added to queue by " + user_name;
                new_edit.editor = user_name;

                new_edit.newLetter = lucky.letterMessage;
                new_edit.previousLetter = lucky.letterMessage;
                new_edit.status = "accepted";
                new_edit.letterID = lucky.Id;
                new_edit.editDate = DateTime.UtcNow;

                Queued new_queued_letter = new Queued();

                new_queued_letter.AddedToQueueDate = DateTime.UtcNow;
                new_queued_letter.LetterID = lucky.Id;
                new_queued_letter.PostDate = DateTime.UtcNow;
                new_queued_letter.QueueID = lucky.Id;
                new_queued_letter.State = 0;

                db_mssql.Queueds.Add(new_queued_letter);
                db_mssql.edits.Add(new_edit);
                db_mssql.SaveChanges();

            }

        }

        public Letter PublishQueue()
        {

            db_mssql db_mssql = new db_mssql();
            db_mysql db_mysql = new db_mysql();

            List<Queued> qry = (from m in db_mssql.Queueds where m.State == 0 orderby m.LetterID ascending select m).ToList();
            if (qry.Count() == 0)
            {
                return null;
            }

            Queued latest_unpublished_queued = qry.First();

            letter latest_front_page = (from l in db_mysql.letters where l.letterLevel == 1 orderby l.letterPostDate descending select l).First();
            letter lucky_letter = (from l in db_mysql.letters where l.Id == latest_unpublished_queued.LetterID select l).FirstOrDefault();

            if (lucky_letter == null)
            {
                return null;
            }

            db_mysql.letters.Attach(lucky_letter);
            var letter = db_mysql.Entry(lucky_letter);
            letter.Property(e => e.letterLevel).IsModified = true;
            lucky_letter.letterLevel = 1;

            db_mssql.Queueds.Attach(latest_unpublished_queued);
            var queued = db_mssql.Entry(latest_unpublished_queued);
            queued.Property(e => e.State).IsModified = true;
            latest_unpublished_queued.State = 1;

            db_mysql.SaveChanges();
            db_mssql.SaveChanges();

            edit new_edit = new edit();
            new_edit.editComment = "Published from queue.";
            new_edit.previousLetter = lucky_letter.letterMessage;
            new_edit.newLetter = lucky_letter.letterMessage;
            new_edit.editDate = DateTime.Now;
            new_edit.letterID = lucky_letter.Id;
            new_edit.status = "accepted";
            new_edit.editor = "auto post";
            db_mssql.edits.Add(new_edit);
            db_mssql.SaveChanges();

            return Mapper.Map<letter, Letter>(lucky_letter);
        }
    }
}
