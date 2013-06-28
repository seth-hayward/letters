using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using letterstocrushes.Core.Model;
using letterstocrushes.Core.Interfaces;

namespace letterstocrushes.Core.Services
{
    public class ChatService
    {

        private readonly IQueryChats _queryChats;
        public ChatService(IQueryChats queryChats)
        {
            _queryChats = queryChats;
        }

        public List<Chat> PopulateChatMessagesFromDatabase()
        {
            return _queryChats.PopulateChatMessagesFromDatabase();
        }

        public void AddChatToDatabase(Chat chat)
        {
            _queryChats.AddChatToDatabase(chat);
        }

    }
}
