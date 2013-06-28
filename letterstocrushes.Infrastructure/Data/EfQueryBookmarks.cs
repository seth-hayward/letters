using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using letterstocrushes.Core.Interfaces;
using letterstocrushes.Core.Model;

namespace letterstocrushes.Infrastructure.Data
{
    public class EfQueryBookmarks : IQueryBookmarks
    {

        private db_mssql _db_mssql;
        private db_mysql _db_mysql;
        public db_mssql db_mssql
        {
            get
            {
                if (_db_mssql == null)
                {
                    _db_mssql = new db_mssql();
                }

                return _db_mssql;
            }
            set
            {
                _db_mssql = value;
            }
        }
        public db_mysql db_mysql
        {
            get
            {
                if (_db_mysql == null)
                {
                    _db_mysql = new db_mysql();
                }

                return _db_mysql;
            }
            set
            {
                _db_mysql = value;
            }
        }

        public void Add(int id, string user_id)
        {
            bookmark b = new bookmark();

            b.UserGuid = user_id;
            b.LetterId = id;
            b.RevisedDate = DateTime.UtcNow;
            b.AddDate = DateTime.UtcNow;
            b.Visible = 1;

            db_mssql.bookmarks.Add(b);
            db_mssql.SaveChanges();
        }

        public Boolean Hide(int id, string user_id)
        {

            bookmark bookmark = (from m in db_mssql.bookmarks where m.UserGuid.Equals(user_id) && m.BookmarkId.Equals(id) select m).FirstOrDefault();

            if (bookmark != null)
            {

                db_mssql.bookmarks.Attach(bookmark);
                var entry = db_mssql.Entry(bookmark);
                entry.Property(e => e.Visible).IsModified = true;
                entry.Property(e => e.RevisedDate).IsModified = true;

                bookmark.Visible = 0;
                bookmark.RevisedDate = DateTime.UtcNow;

                db_mssql.SaveChanges();

                return true;
            }
            else
            {
                return false;
            }

        }

        public int getBookmarkCountByUser(string user_guid)
        {
            return (from m in db_mssql.bookmarks where (m.UserGuid == user_guid) && (m.Visible == 1) select m).Count();
        }

        public List<Letter> getBookmarksByUser(string user_guid, int page)
        {

            EfQueryLetters _queryLetters = new EfQueryLetters();
            List<bookmark> bookmarks = new List<bookmark>();
            List<Letter> letters = new List<Letter>();
            bookmarks = (from m in db_mssql.bookmarks where (m.UserGuid == user_guid) && (m.Visible == 1) orderby m.BookmarkId descending select m)
                 .Skip((page - 1) * 10).Take(10).ToList();

            foreach (bookmark tmp_bookmark in bookmarks)
            {

                Letter tmp_letter = _queryLetters.getLetter(tmp_bookmark.LetterId);

                if (tmp_letter != null)
                {
                    tmp_letter.letterLanguage = tmp_bookmark.BookmarkId.ToString();
                    letters.Add(tmp_letter.Clone());
                }

            }

            return letters;
        }
    }
}
