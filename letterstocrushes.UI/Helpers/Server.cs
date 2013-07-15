using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Collections;
using System.Threading.Tasks;
using Microsoft.AspNet.SignalR;
using System.Text;
using System.Diagnostics;
using letterstocrushes.Core.Model;
using letterstocrushes.Core.Services;

namespace letterstocrushes
{
    public class MyConnection : PersistentConnection
    {
        protected override Task OnReceived(IRequest request, string connectionId, string data)
        {
            // Broadcast data to all clients
            return Connection.Broadcast(data);
        }
    }

    public class Chat : Hub
    {

        private static DateTime started = DateTime.Now;
        private static int max;

        public static DateTime Started
        {
            get
            {
                return started;
            }
            set
            {
                started = value;
            }
        }

        public static int Max
        {
            get
            {
                return max;
            }
            set
            {
                max = value;
            }
        }

        private static List<ChatMessage> messages;
        private static Dictionary<String, ChatVisitor> _visitors;

        private static Core.Services.ChatService _chatService;
        public static Core.Services.ChatService chatService
        {
            get
            {
                if (_chatService == null)
                {
                    _chatService = new Core.Services.ChatService(new Infrastructure.Data.EfQueryChats());
                }
                return _chatService;
            }
            set
            {
                _chatService = value;
            }
        }

        public static List<ChatMessage> Messages
        {
            get
            {
                if (messages == null)
                {
                    messages = new List<ChatMessage>();

                    _chatService = new Core.Services.ChatService(new Infrastructure.Data.EfQueryChats());

                    List<Core.Model.Chat> database_chats = new List<Core.Model.Chat>();
                    database_chats = _chatService.PopulateChatMessagesFromDatabase("1");
                    
                    foreach (Core.Model.Chat msg in database_chats)
                    {
                        ChatMessage new_mgs = new ChatMessage();
                        new_mgs.Room = msg.Room;
                        new_mgs.ChatDate = msg.ChatDate;
                        new_mgs.Message = msg.Message;
                        new_mgs.Nick = msg.Nick;
                        new_mgs.StoredInDB = true;
                        Messages.Add(new_mgs);
                    }

                    ChatMessage reboot = new ChatMessage();
                    reboot.ChatDate = DateTime.UtcNow;
                    reboot.Room = "1";
                    reboot.Nick = "chatbot";
                    reboot.Message = "The server was rebooted. If it's acting weird, it's seth's fault.";
                    reboot.StoredInDB = false;
                    Messages.Add(reboot);                    
                }
                return messages;
            }
            set
            {
                messages = value;

                // now we just want to check to make sure that
                // there are not more than 200 chat messages
                // per room... if there are,
                // we want to remove the oldest ones


                // how processing intensive is this?
                // it's probably way worse to just
                // take up tons of memory... but this may be slow too
                // -- whatever, gogogogo

                // steps:
                // - get list of rooms in use
                // - loop through each room in use
                //     - get count of messages in room
                //     - remove oldest if there are more than 200 in each room

                List<String> room_list = (from m in messages select m.Room).Distinct().ToList();

                foreach (string room in room_list)
                {
                    int message_count = getRoomMessageCount(room);

                    while (getRoomMessageCount(room) > 200)
                    {
                        // remove oldest from the room
                        ChatMessage oldest = (from m in messages where m.Room.Equals(room) orderby m.ChatDate ascending select m).FirstOrDefault();

                        if(oldest != null) {
                            messages.Remove(oldest);
                        }
                    }

                }

            }
        }


        public static int getRoomMessageCount(string room)
        {
            return (from m in messages where m.Room.Equals(room) select m).Count();
        }

        public static Dictionary<String, ChatVisitor> Visitors
        {
            get
            {
                if (_visitors == null)
                {
                    _visitors = new Dictionary<String,ChatVisitor>();
                }
                return _visitors;
            }
            set
            {
                _visitors = value;
            }
        }

        public void LeaveAndJoinGroup(string room)
        {

            // get the current user
            ChatVisitor current_user = Visitors[Context.ConnectionId];

            // send a message to that group that the user has left
            ChatMessage chat = new ChatMessage();
            chat.ChatDate = DateTime.UtcNow;
            chat.Message = current_user.Handle + " left this room and joined room #" + room + ". Type <b>/join " + room + "</b> to do the same.";
            chat.Room = current_user.Room;
            chat.Nick = "Left";

            Clients.OthersInGroup(current_user.Room).addMessage(chat);

            Messages.Add(chat);

            // leave the user's current room
            Groups.Remove(current_user.ConnectionId, current_user.Room);

            // tell the client to clear it's history
            ChatMessage reset = new ChatMessage();
            reset.Room = "reset-channel";
            Clients.Caller.addMessage(reset);

            // join the new room
            JoinGroup(room);

        }

        public void AnnounceRestart()
        {
            Clients.All.errorMessage("The server was rebooted. You may need to refresh this page.");
        }

        public void JoinGroup(string room)
        {

            // get the current user
            ChatVisitor current_user = Visitors[Context.ConnectionId];

            current_user.Room = room;

            ChatMessage announced = new ChatMessage();
            announced.Nick = "Joined:";
            announced.Message = current_user.Handle;
            announced.ChatDate = DateTime.UtcNow;
            announced.Room = room; // well this seems silly, but want to make sure numbers work

            // add to the group
            Groups.Add(Context.ConnectionId, room);

            // send the message to the group about the new person
            Clients.OthersInGroup(room).addMessage(announced);

            StringBuilder who_is_here = new StringBuilder();
            List<ChatVisitor> people_in_room = (from m in Visitors.Values where m.Room.Equals(room) select m).ToList();
            foreach (ChatVisitor nick in people_in_room)
            {
                who_is_here.Append(nick.Handle + ", ");
            }
            who_is_here.Remove(who_is_here.Length - 2, 2);

            ChatMessage chat = new ChatMessage();
            chat.Nick = "In chat room " + room + ":";
            chat.Message = who_is_here.ToString();
            chat.ChatDate = DateTime.UtcNow;

            List<ChatMessage> chat_backlog = new List<ChatMessage>();
            chat_backlog = (from m in Messages where m.Room.Equals(room) orderby m.ChatDate descending select m).Take(200).ToList();
            chat_backlog.Reverse();

            chat_backlog.Add(chat);

            Clients.Caller.addBacklog(chat_backlog);

            Messages.Add(announced);

        }

        public void Join(string name)
        {

            // check to see if a user with this name is on the chat
            ChatVisitor existing_user = (from m in Visitors.Values where m.Handle.Equals(name) select m).FirstOrDefault();

            if (existing_user != null)
            {
                // allow the person back in with this new name, but
                // update the connection id

                Debug.Print("previous existing user detected: " + existing_user.ConnectionId);

                // maybe we remove it now instead?
                Visitors.Remove(existing_user.ConnectionId);

                // let's send the connection a message just
                // in case it is still active
                Clients.Client(existing_user.ConnectionId).errorMessage("Another person has logged in with your name. You have been disconnected.");
                //Clients.AllExcept(existing_user.ConnectionId).addMessage(existing_user.Handle + "has logged in with the same handle. The previous handle has been disconnected");
            }

            Clients.Caller.enterChat(1);

            // create a new user
            ChatVisitor chatter = new ChatVisitor();
            chatter.ConnectionId = Context.ConnectionId;
            chatter.Handle = name;
            chatter.Room = "1";

            if (Visitors == null)
            {
                Debug.Print("Visitors was null.");
            }

            if (chatter == null || chatter.ConnectionId == null)
            {
                Debug.Print("Chatter or chatter.connectionId was null");
            }

            if (Visitors.ContainsKey(chatter.ConnectionId))
            {
                // this really shouldn't happen, but it seems to happen
                // after a reboot
                Visitors[chatter.ConnectionId] = chatter;
            } else {
                Visitors.Add(chatter.ConnectionId, chatter);
            }

            JoinGroup("1");

            if (Max < Visitors.Count) { Max = Visitors.Count; }

            if (Started == null)
            {
                Started = DateTime.UtcNow;
            }

            TimeSpan uptime = DateTime.Now - Started;
            Clients.Group("admins").addMessage(String.Format("{0} active users. {1} max users. uptime is {2}", Visitors.Count, Max, uptime.Hours + " hours, " + uptime.Minutes + " minutes."));

        }

        public void UpdateDatabase()
        {

            List<ChatMessage> temp_messages = new List<ChatMessage>(Messages);

            foreach (ChatMessage msg in temp_messages)
            {

                if (msg.StoredInDB == false)
                {
                    Core.Model.Chat chat = new Core.Model.Chat();
                    chat.ChatDate = msg.ChatDate;
                    chat.Message = msg.Message;
                    chat.Nick = msg.Nick;
                    chat.Room = msg.Room;
                    _chatService.AddChatToDatabase(chat);
                    msg.StoredInDB = true;
                }

            }

            Messages = temp_messages;
        
        }

        public void Send(string message)
        {

            // ignore blank messages
            if (message.Length == 0)
            {
                return;
            }

            // get the current user
            ChatVisitor current_user;
            bool get_user = Visitors.TryGetValue(Context.ConnectionId, out current_user);

            ChatMessage error = new ChatMessage();
            error.ChatDate = DateTime.UtcNow;
            error.Nick = "";

            if (get_user == false)
            {
                error.Message = "Can't find user.";
                Clients.Caller.addMessage(error);
                return;
            }

            ChatMessage chat = new ChatMessage();
            chat.Nick = current_user.Handle + ":";
            chat.Message = message;
            chat.ChatDate = DateTime.UtcNow;
            chat.StoredInDB = false;
            chat.Room = current_user.Room;

            bool handled = false;

            //
            //
            // COMMAND HANDLER!!!!
            //  - available commands:
            //    1: /join n             # where n is a number less than 10, join a room
            //

            if (message.StartsWith("/join"))
            {

                string room = message.Replace("/join ", "");
                int room_number;
                if (int.TryParse(room, out room_number) == false)
                {
                    room_number = 1;
                    error.Message = "Unable to join room. Not a valid room number: <b>" + room + "</b>.";
                    Clients.Caller.addMessage(error);
                    handled = true;
                }
                else
                {
                    LeaveAndJoinGroup(room_number.ToString());
                    handled = true;
                }

            }

            if (handled == false)
            {
                // Call the addMessage method on all the clients in the room
                Clients.Group(current_user.Room).addMessage(chat);

                Messages.Add(chat);
            }

            // every message, update the database
            UpdateDatabase();

            TimeSpan uptime = DateTime.Now - Started;
            Clients.Group("admins").addMessage(String.Format("{0} active users. {1} max users. uptime is {2}", Visitors.Count, Max, uptime.Hours + " hours, " + uptime.Minutes + " minutes."));


        }

        public void Admin(string password)
        {
            if (password == "lolcats")
            {
                Groups.Add(Context.ConnectionId, "admins");
                Clients.Caller.addBacklog(Messages);
                TimeSpan uptime = DateTime.Now - Started;
                Clients.Caller.addMessage(String.Format("{0} active users. {1} max users. uptime is {2}", Visitors.Count, Max, uptime.Hours + " hours, " + uptime.Minutes + " minutes."));

            }
        }

        public override Task OnConnected()
        {
            return Clients.All.joined(Context.ConnectionId, DateTime.Now.ToString());
        }

        public override Task OnDisconnected()
        {

            // get the current user
            ChatVisitor current_user;
            bool get_user = Visitors.TryGetValue(Context.ConnectionId, out current_user);

            if (get_user == true)
            {

                ChatMessage chat = new ChatMessage();
                chat.Nick = "Left:";
                chat.Message = current_user.Handle;
                chat.ChatDate = DateTime.UtcNow;
                chat.Room = current_user.Room; // not sure this is necessary when sending to client

                Clients.Group(current_user.Room).addMessage(chat);

                Messages.Add(chat);
                Visitors.Remove(current_user.ConnectionId);

                TimeSpan uptime = DateTime.Now - Started;
                Clients.Group("admins").addMessage(String.Format("{0} active users. {1} max users. uptime is {2}", Visitors.Count, Max, uptime.Hours + " hours, " + uptime.Minutes + " minutes."));

            }

            return Clients.All.leave(Context.ConnectionId, DateTime.Now.ToString());
        }

        public override Task OnReconnected()
        {
            return Clients.All.rejoined(Context.ConnectionId, DateTime.Now.ToString());
        }

    }

    public class Visitor
    {
        public string Connection_Id { get; set; }
        public string Page_Type { get; set; }
        public DateTime Created { get; set; }
        public int Page { get; set; }
        public int Start_Id { get; set; }
        public int End_Id { get; set; }
    }

    public class ChatVisitor
    {

        public string ConnectionId { get; set; }
        public string Handle { get; set; }
        public string Room { get; set; }
        public int Offset { get; set; }

    }

    public class ChatMessage
    {
        public DateTime ChatDate { get; set; }
        public string Nick { get; set; }
        public string Message { get; set; }
        public string Room { get; set; }
        public Boolean StoredInDB { get; set; }
    }

    public class VisitorUpdate : Hub
    {

        private static DateTime started = DateTime.Now;
        private static int max;
        private static int global_connections;
        private static Hashtable connection_ids;

        public static DateTime Started
        {
            get
            {
                return started;
            }
            set
            {
                started = value;
            }
        }

        public static int Max
        {
            get
            {
                return max;
            }
            set
            {
                max = value;
            }
        }

        public static int Global_Connections
        {
            get
            {
                return global_connections;
            }
            set
            {
                global_connections = value;
            }
        }

        public static Hashtable Connection_Ids
        {
            get
            {
                if (connection_ids == null)
                {
                    connection_ids = new Hashtable();
                }

                return connection_ids;
            }
            set
            {
                connection_ids = value;
            }
        }

        public void Join(int start_id, int end_id, string page_type)
        {

            if(Connection_Ids.Contains(Context.ConnectionId)) {
                return;
            }
            Global_Connections++;

            Visitor current_visitor = new Visitor();
            current_visitor.Connection_Id = Context.ConnectionId;
            current_visitor.Page_Type = page_type;
            current_visitor.Start_Id = start_id;
            current_visitor.End_Id = end_id;
            current_visitor.Created = DateTime.Now;

            Connection_Ids.Add(Context.ConnectionId, current_visitor);

            if (Max < Connection_Ids.Count) { Max = Connection_Ids.Count; }

            if (Started == null)
            {
                Started = DateTime.UtcNow;
            }

            TimeSpan uptime = DateTime.Now - Started;
            Clients.Group("admins").addMessage(String.Format("{0} active users. {1} max users. uptime is {2}", Connection_Ids.Count, Max, uptime.Hours + " hours, " + uptime.Minutes + " minutes."));

        }

        public int GetActives()
        {
            return Global_Connections;
        }

        public void Vote(int letter_id, string c)
        {

            // loop through all of the Connection_Ids,
            // find the users with letter_id between start and end
            // send messages to each of those connections

            System.Diagnostics.Debug.Print("Voter connection id: " + c);
            Visitor voter = (Visitor)Connection_Ids[c];

            List<Visitor> connections_to_remove = new List<Visitor>();


            foreach (Visitor user in Connection_Ids.Values)
            {
                System.Diagnostics.Debug.Print("Testing " + letter_id + ": " + user.Connection_Id + " (" + user.Start_Id + "," + user.End_Id + ")");
                if (user.Start_Id >= letter_id && user.End_Id <= letter_id && user.Connection_Id != c && user.Page_Type == voter.Page_Type)
                {
                    Clients.Client(user.Connection_Id).addMessage(letter_id);
                    System.Diagnostics.Debug.Print("Sent message to " + user.Connection_Id);
                }

                TimeSpan timeout = DateTime.Now.Subtract(user.Created);
                if (timeout.Minutes > 60)
                {
                    // timeout after 60 minutes
                    System.Diagnostics.Debug.Print("Visitor created too long ago, removing: " + user.Connection_Id);
                    connections_to_remove.Add(user);
                }
            }

            foreach (Visitor user in connections_to_remove)
            {
                Connection_Ids.Remove(user.Connection_Id);
            }

            System.Diagnostics.Debug.Print(c + ": voted on letter "
                + letter_id + " - total users: " + Connection_Ids.Count);
        }

        public void Admin(string password)
        {
            if (password == "lolcats")
            {
                Groups.Add(Context.ConnectionId, "admins");

                TimeSpan uptime = DateTime.Now - Started;
                Clients.Caller.addMessage(String.Format("{0} active users. {1} max users. uptime is {2}", Connection_Ids.Count, Max, uptime.Hours + " hours, " + uptime.Minutes + " minutes."));

            }
        }


        public void Send(string message)
        {
            // Call the addMessage method on all clients            
            Clients.All.addMessage(message);
        }

        public override Task OnConnected()
        {
            return Clients.All.joined(Context.ConnectionId, DateTime.Now.ToString());
        }

        public override Task OnDisconnected()
        {
            if (Global_Connections > 0) {
                Global_Connections--;
            }

            Connection_Ids.Remove(Context.ConnectionId);

            TimeSpan uptime = DateTime.Now - Started;
            Clients.Group("admins").addMessage(String.Format("{0} active users. {1} max users. uptime is {2}", Connection_Ids.Count, Max, uptime.Hours + " hours, " + uptime.Minutes + " minutes."));

            return Clients.All.leave(Context.ConnectionId, DateTime.Now.ToString());
        }

        public override Task OnReconnected()
        {

            if(Connection_Ids.Contains(Context.ConnectionId)) {
                return Clients.All.rejoined(Context.ConnectionId, DateTime.Now.ToString());
            } else {
                return Clients.All.joined(Context.ConnectionId, DateTime.Now.ToString());
            }

        }

    }
}