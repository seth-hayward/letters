using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using letterstocrushes.Core.Model;

namespace letterstocrushes.Core.Interfaces
{
    public interface IQueryComments
    {
        Comment getComment(int id);
        List<Comment> getComments(int id);
        List<Comment> getRecentComments(int page);
        void AddComment(Comment comment, Letter letter);
        Boolean Unsubscribe(string email, int letter_id);
        Boolean editComment(string commentText, int id, string commenter_guid, string user_name);
        Boolean hideComment(int id, string commenter_guid, string user_name);
        int getCommentCount();
    }
}
