using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using letterstocrushes.Core.Model;

namespace letterstocrushes.Core.Interfaces
{
    public interface IQueryBookmarks
    {
        void Add(int id, string user_id);
        Boolean Hide(int id, string user_id);
        int getBookmarkCountByUser(string user_guid);
        List<Letter> getBookmarksByUser(string user_guid, int page);
    }
}
