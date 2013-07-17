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
    public class EfQueryChats : IQueryChats
    {

        public EfQueryChats()
        {
            Mapper.CreateMap<Chat, chat>();
            Mapper.CreateMap<chat, Chat>();
        }

        public List<Chat> PopulateChatMessagesFromDatabase(string room)
        {

            // check the database to see if there
            // are any -- load the most recent 200
            // if yes

            db_mysql db = new db_mysql();

            chat most_recent = (from m in db.chats orderby m.id descending select m).First();

            List<chat> database_chats = new List<chat>();

            if (room == "")
            {
                database_chats = (from m in db.chats
                                  orderby m.id ascending
                                  where m.id > most_recent.id - 200
                                  select m).ToList();

            }
            else
            {
                database_chats = (from m in db.chats
                                  orderby m.id ascending
                                  where m.id > most_recent.id - 200
                                  && m.Room.Equals(room)
                                  select m).ToList();
            }

            return Mapper.Map<List<chat>, List<Chat>>(database_chats);
        }

        public void AddChatToDatabase(Chat chat)
        {
            chat transposed = Mapper.Map<Chat, chat>(chat);
            db_mysql db = new db_mysql();
            db.chats.Add(transposed);
            db.SaveChanges();
        }

        public string GetStats(string name)
        {

            String stats = "";
            db_mysql db = new db_mysql();

            if (name == "")
            {
                // return general stats
                stats = (from m in db.chats select m).Count() + " chats total.";
            }
            else
            {
                // ugh bad seth
                String fixed_name = name + ":";

                // return user based stats
                stats = (from m in db.chats where m.Nick.ToLower().Equals(fixed_name.ToLower()) select m).Count() + " chats from '" + name + "'";
            }

            return stats;

        }
    }
}
