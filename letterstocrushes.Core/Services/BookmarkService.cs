using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using letterstocrushes.Core.Interfaces;
using letterstocrushes.Core.Model;

namespace letterstocrushes.Core.Services
{
    public class BookmarkService
    {

        private readonly IQueryBookmarks _queryBookmarks;
        public BookmarkService(IQueryBookmarks queryBookmarks)
        {
            _queryBookmarks = queryBookmarks;
        }

        public void Add(int id, string user_id)
        {
            _queryBookmarks.Add(id, user_id);
        }

        public Boolean Hide(int id, string user_id)
        {
            return _queryBookmarks.Hide(id, user_id);
        }

        public int getBookmarkCountByUser(string user_guid)
        {
            return _queryBookmarks.getBookmarkCountByUser(user_guid);
        }

        public List<Letter> getBookmarksByUser(string user_guid, int page)
        {
            return _queryBookmarks.getBookmarksByUser(user_guid, page);
        }

    }
}
