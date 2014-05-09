using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using letterstocrushes.Core.Model;
using letterstocrushes.Core.Interfaces;

namespace letterstocrushes.Infrastructure.Data
{
    public class EfQueryVotes : IQueryVotes
    {

        public int Vote(int letter_id, string user_ip)
        {

            db_mysql db_mysql = new db_mysql();
            db_mssql db_mssql = new db_mssql();

            letter letterToView = (from l in db_mysql.letters where l.Id == letter_id select l).FirstOrDefault();

            vote loveVote = new vote();

            //loveVote.letterID = letter_id;
            //loveVote.voterIP = user_ip;
            //loveVote.voteValue = 1;
            //loveVote.voteDate = DateTime.Now;

            //db_mssql.votes.Add(loveVote);
            //db_mssql.SaveChanges();

            db_mysql.letters.Attach(letterToView);
            var letter = db_mysql.Entry(letterToView);

            letter.Property(e => e.letterUp).IsModified = true;
            letterToView.letterUp = (short)(letterToView.letterUp + 1);

            db_mysql.SaveChanges();

            return (int)letterToView.letterUp;

        }

    }
}
