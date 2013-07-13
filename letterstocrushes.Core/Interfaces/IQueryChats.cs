using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using letterstocrushes.Core.Model;

namespace letterstocrushes.Core.Interfaces
{
    public interface IQueryChats
    {
        List<Chat> PopulateChatMessagesFromDatabase(string room);
        void AddChatToDatabase(Chat chat);
    }
}
