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
        public string IP { get; set; }
    }

    public class ChatMessage
    {
        public DateTime ChatDate { get; set; }
        public string Nick { get; set; }
        public string Message { get; set; }
        public string Room { get; set; }
        public Boolean StoredInDB { get; set; }
        public string IP { get; set; }
    }

    public class VisitorUpdate : Hub
    {

        private static DateTime started = DateTime.Now;
        private static DateTime _chatStarted;
        private static int max;
        private static int global_connections;
        private static Hashtable connection_ids;

        public static DateTime chatStarted
        {
            get
            {
                if (_chatStarted == null)
                {
                    _chatStarted = DateTime.Now;
                }

                return _chatStarted;
            }
            set
            {
                _chatStarted = value;
            }
        }

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
                Clients.Caller.addBacklog(Messages);
            }
        }


        public void Send(string message)
        {

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

            }

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

        private static Core.Services.BlockService _blockService;
        public static Core.Services.BlockService blockService
        {
            get
            {
                if (_blockService == null)
                {
                    _blockService = new Core.Services.BlockService(new Infrastructure.Data.EfQueryBlocks());
                }
                return _blockService;
            }
            set
            {
                _blockService = value;
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

                        // now, we only want to add chats to this list
                        // if they are AFTER chatStarted... this allow us
                        // to clear the log without updating the database
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

                        if (oldest != null)
                        {
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
                    _visitors = new Dictionary<String, ChatVisitor>();
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
            chat.Message = current_user.Handle + " left this room to go somewhere else.";
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
            JoinGroup(room, true);

        }

        public void AnnounceRestart()
        {
            Clients.All.errorMessage("The server was rebooted. You may need to refresh this page.");
        }

        public void JoinGroup(string room, bool display_welcome)
        {

            if (Visitors.ContainsKey(Context.ConnectionId) == false)
            {
                Debug.Print("Phantom user tried to connect.");
                return;
            }

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

            if (display_welcome == true)
            {
                Clients.OthersInGroup(room).addMessage(announced);
                Clients.OthersInGroup(room).addSimpleMessage(announced.Nick + " " + announced.Message);
            }

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
            StringBuilder simple_chat_backlog = new StringBuilder();
            
            chat_backlog = (from m in Messages where m.Room.Equals(room) orderby m.ChatDate descending select m).Take(200).ToList();
            chat_backlog.Reverse();

            chat_backlog.Add(chat);

            foreach (var c in chat_backlog)
            {
                simple_chat_backlog.AppendLine(c.Nick + " " + c.Message);
            }

            Clients.Caller.addBacklog(chat_backlog);
            Clients.Caller.addSimpleBacklog(simple_chat_backlog.ToString());

            Messages.Add(announced);

        }

        public void Join(string name)
        {

            bool display_welcome = true;

            String user_ip = HttpContext.Current.Request.UserHostAddress;

            List<Block> blocked_ips = blockService.getBlocks(blockType.blockIP, blockWhat.blockChat);

            if ((from m in blocked_ips select m.Value).Contains(user_ip))
            {
                Clients.Caller.errorMessage("You are in timeout.");
                return;
            }


            if (name == null) { return; }

            // check to see if a user with this name is on the chat
            ChatVisitor existing_user = (from m in Visitors.Values where m.Handle.Equals(name) select m).FirstOrDefault();

            // check to see if this connection id already has a user associated with it...

            if (existing_user != null)
            {

                // allow the person back in with this new name, but
                // update the connection id

                // BUT ONLY ... IF... that old connection no longer exists?

                display_welcome = false;

                String previous_id = existing_user.ConnectionId;

                // remove the old id, we're going to 
                // add a new one after this
                Visitors.Remove(previous_id);

                existing_user.ConnectionId = Context.ConnectionId;

                Visitors.Add(Context.ConnectionId, existing_user);

                //Clients.Client(previous_id).errorMessage("Someone else has reconnected with your name. This session has been disconnected.");

                // let's send the connection a message just
                // in case it is still active
                //ChatMessage join_error = new ChatMessage();
                //join_error.Nick = "chatbot:";
                //join_error.Message = existing_user.Handle + " reconnected.";

                //Clients.AllExcept(existing_user.ConnectionId).addMessage(join_error);
                //Clients.AllExcept(existing_user.ConnectionId).addSimpleMessage(join_error.Nick + " " + join_error.Message);

            }

            Clients.Caller.enterChat(1);

            // create a new user
            ChatVisitor chatter = new ChatVisitor();
            chatter.ConnectionId = Context.ConnectionId;
            chatter.Handle = name;
            chatter.Room = "1";
            chatter.IP = user_ip;

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
            }
            else
            {

                try
                {
                    Visitors.Add(chatter.ConnectionId, chatter);
                }
                catch (Exception ex)
                {
                    //Visitors.Add(chatter.ConnectionId, chatter);
                }

            }

            JoinGroup("1", display_welcome);

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
                    chat.IP = msg.IP;
                    _chatService.AddChatToDatabase(chat);
                    msg.StoredInDB = true;
                }

            }

            Messages = temp_messages;

        }

        public void AnnounceStatusChange(string ip)
        {


            foreach (ChatVisitor k in Visitors.Values) {
                if (k.IP == ip)
                {
                    Clients.Client(k.ConnectionId).reloadChat();
                }
            }

        }

        public void SendChat(string message)
        {
            // chat message
            
            String user_ip = HttpContext.Current.Request.UserHostAddress;

            List<Block> blocked_ips = blockService.getBlocks(blockType.blockIP, blockWhat.blockChat);

            if ((from m in blocked_ips select m.Value).Contains(user_ip))
            {
                Clients.Caller.errorMessage("You are in timeout.");
                return;
            }


            ChatMessage error = new ChatMessage();
            error.ChatDate = DateTime.UtcNow;
            error.Nick = "";

            // ignore blank messages and long messages
            if (message.Length == 0)
            {
                return;
            }

            if (message.Length > 2000)
            {
                error.Message = "Message was too long.";
                Clients.Caller.addMessage(error);
                return;
            }

            // get the current user
            ChatVisitor current_user;
            bool get_user = Visitors.TryGetValue(Context.ConnectionId, out current_user);


            if (get_user == false)
            {
                error.Message = "Can't find user.";
                Clients.Caller.addMessage(error);
                Clients.Caller.addSimpleMessage("chatbot: Can't find user.");
                return;
            }


            ChatMessage chat_1 = new ChatMessage();
            ChatMessage chat = new ChatMessage();
            chat.Nick = current_user.Handle + ":";
            chat.Message = message;
            chat.ChatDate = DateTime.UtcNow;
            chat.StoredInDB = false;
            chat.Room = current_user.Room;
            chat.IP = HttpContext.Current.Request.UserHostAddress;

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
                    Clients.Caller.addSimpleMessage("chatbot: " + error.Message);
                    handled = true;
                }
                else
                {
                    LeaveAndJoinGroup(room_number.ToString());
                    handled = true;
                }

            }

            if (message.StartsWith("/w ")) {
                // /w {username} message
                // WHISPER TIME

                string[] message_parameters = message.Split(' ');
                string whisper_to = message_parameters[1];
                string extracted_message = message.Replace("/w " + whisper_to + " ", "");

                bool found_person = false;

                chat_1.Nick = current_user.Handle + " *whisper*:";
                chat_1.Message = extracted_message;
                chat_1.ChatDate = DateTime.UtcNow;
                chat_1.StoredInDB = false;
                chat_1.Room = current_user.ConnectionId;

                chat.ChatDate = DateTime.UtcNow;
                chat.Nick = current_user.Handle + " *whispered to " + whisper_to + "*:";
                chat.Message = extracted_message;
                chat.StoredInDB = false;
                chat.Room = current_user.ConnectionId;

                foreach (ChatVisitor c in Visitors.Values)
                {

                    if (c.Handle == whisper_to)
                    {
                        found_person = true;

                        Clients.Client(c.ConnectionId).addMessage(chat_1);
                        Clients.Client(c.ConnectionId).addSimpleMessage(chat_1.Nick + " " + chat_1.Message);

                        Clients.Caller.addMessage(chat);
                        Clients.Caller.addSimpleMessage(chat.Nick + " " + chat.Message);
                    }


                }

                if (found_person == false)
                {
                    chat_1.Nick = "chatbot:";
                    chat_1.Message = "could not find this user: " + whisper_to;
                    Clients.Caller.addMessage(chat_1);
                    Clients.Caller.addSimpleMessage(chat_1.Nick + " " + chat_1.Message);
                }

                handled = true;
            }

            //if (message.StartsWith("/clear"))
            //{
            //    if (HttpContext.Current.User.IsInRole("Mod"))
            //    {


            //        ChatMessage reboot = new ChatMessage();

            //        // this has to be Now because the rest of the
            //        // uptime stuff uses Now instead of UtcNow

            //        DateTime lastChatDate = Messages.Last().ChatDate;
            //        chatStarted = lastChatDate;

            //        HttpContext.Current.Cache.Insert("startChatDate", lastChatDate, null, DateTime.UtcNow.AddDays(1), TimeSpan.Zero);

            //        // reset messages list
            //        Messages = new List<ChatMessage>();

            //        reboot.ChatDate = DateTime.UtcNow;
            //        reboot.Room = "1";
            //        reboot.Nick = "chatbot:";
            //        reboot.Message = "The chat log was cleared by a moderator.";
            //        reboot.StoredInDB = false;
            //        Clients.Caller.addMessage(reboot);
            //        Clients.Caller.addSimpleMessage(reboot.Nick + " " + reboot.Message);


            //    }
            //    else
            //    {
            //        ChatMessage not_mod = new ChatMessage();
            //        not_mod.Nick = "chatbot:";
            //        not_mod.Message = "You must be logged into a mod account to run this command.";
            //        Clients.Caller.addMessage(not_mod);
            //        Clients.Caller.addSimpleMessage(not_mod.Nick + " " + not_mod.Message);
            //    }
            //    handled = true;
            //}

            if (message.StartsWith("/help"))
            {


                chat_1.Nick = "chatbot:";
                chat_1.Message = "Hey, friend! Welcome to the chat. Here's some secret commands you can do: ";
                chat_1.ChatDate = DateTime.UtcNow;
                chat_1.StoredInDB = false;
                chat_1.Room = current_user.Room;

                Clients.Caller.addMessage(chat_1);
                Clients.Caller.addSimpleMessage(chat_1.Nick + " " + chat_1.Message);

                chat_1.Message = "/help [type this to show this message]";

                Clients.Caller.addMessage(chat_1);
                Clients.Caller.addSimpleMessage(chat_1.Nick + " " + chat_1.Message);

                chat_1.Message = "/join 2 [type this to join room 2, or any other numbered room]";

                Clients.Caller.addMessage(chat_1);
                Clients.Caller.addSimpleMessage(chat_1.Nick + " " + chat_1.Message);

                chat_1.Message = "/ask Does she think about me? [ask the 8-ball a yes/no question :) ]";

                Clients.Caller.addMessage(chat_1);
                Clients.Caller.addSimpleMessage(chat_1.Nick + " " + chat_1.Message);

                chat_1.Message = "/w seth lol this is a whisper [send a private message to user seth, only seen by you and them]";

                Clients.Caller.addMessage(chat_1);
                Clients.Caller.addSimpleMessage(chat_1.Nick + " " + chat_1.Message);

                handled = true;
            }

            if (message.StartsWith("/ask "))
            {

                message = message.Replace("/ask ", "");

                List<String> magic_8_ball = new List<String>();
                magic_8_ball.Add("It is certain");
                magic_8_ball.Add("It is decidedly so");
                magic_8_ball.Add("Without a doubt");
                magic_8_ball.Add("Yes, definitely");
                magic_8_ball.Add("You may rely on it");
                magic_8_ball.Add("As I see it, yes");
                magic_8_ball.Add("Most likely");
                magic_8_ball.Add("Outlook is good");
                magic_8_ball.Add("Yes");
                magic_8_ball.Add("Signs point to yes");

                magic_8_ball.Add("Reply hazy, try again");
                magic_8_ball.Add("Ask again later");
                magic_8_ball.Add("Better not tell you now");
                magic_8_ball.Add("Cannot predict now");
                magic_8_ball.Add("Concentrate and ask again");


                magic_8_ball.Add("Don't count on it");
                magic_8_ball.Add("My reply is no");
                magic_8_ball.Add("My sources say no");
                magic_8_ball.Add("Outlook not so good");
                magic_8_ball.Add("Very doubtful");

                chat_1.Nick = "chatbot:";
                chat_1.Message = current_user.Handle + " asked the magic 8-ball: " + message;
                chat_1.ChatDate = DateTime.UtcNow;
                chat_1.StoredInDB = false;
                chat_1.Room = current_user.Room;

                // Call the addMessage method on all the clients in the room
                Clients.Group(current_user.Room).addMessage(chat_1);
                Clients.Group(current_user.Room).addSimpleMessage(chat_1.Nick + " " + chat_1.Message);

                ChatMessage chat_2 = new ChatMessage();

                Random rand = new Random();

                chat_2.Nick = "magic 8 ball:";
                chat_2.Message = "And the answer is: " + magic_8_ball[rand.Next(magic_8_ball.Count)];
                chat_2.ChatDate = DateTime.UtcNow;
                chat_2.StoredInDB = false;
                chat_2.Room = current_user.Room;

                Clients.Group(current_user.Room).addMessage(chat_2);
                Clients.Group(current_user.Room).addSimpleMessage(chat_1.Nick + " " + chat_1.Message);

                Messages.Add(chat_1);
                Messages.Add(chat_2);

                handled = true;
            }

            if (message.StartsWith("/stats"))
            {

                String stats_message = "";
                String stats_intro = "General stats: ";
                String check_user = "";
                chat_1 = new ChatMessage();

                if (message != "/stats")
                {
                    check_user = message.Replace("/stats ", "");
                    stats_intro = "Stats for " + check_user + ": ";
                }

                message = _chatService.GetStats(check_user);

                chat_1.Nick = "chatbot:";
                chat_1.Message = stats_intro + message;

                chat_1.ChatDate = DateTime.UtcNow;
                chat_1.StoredInDB = false;
                chat_1.Room = current_user.Room;
                Clients.Caller.addMessage(chat_1);
                Clients.Caller.addSimpleMessage(chat_1.Nick + " " + chat_1.Message);
                handled = true;

            }

            //if (message.StartsWith("/report ")) {
            //    string report_msg = message.Replace("/report ", "");                
            //}

            if (message.StartsWith("/ips"))
            {
                if (HttpContext.Current.User.IsInRole("Mod"))
                {
                    ChatMessage ip_line = new ChatMessage();
                    ip_line.Nick = "chatbot:";
                    ip_line.Message = "Here are the IP address of the " + Visitors.Values.Count + " chatters.";
                    Clients.Caller.addMessage(ip_line);
                    Clients.Caller.addSimpleMessage(ip_line.Nick + " " + ip_line.Message);

                    foreach (ChatVisitor chatter in Visitors.Values)
                    {
                        ip_line = new ChatMessage();
                        ip_line.Message = chatter.IP;
                        ip_line.Nick = chatter.Handle;
                        Clients.Caller.addMessage(ip_line);
                        Clients.Caller.addSimpleMessage(ip_line.Nick + " " + ip_line.Message);
                    }
                }
                else
                {
                    ChatMessage not_mod = new ChatMessage();
                    not_mod.Nick = "chatbot:";
                    not_mod.Message = "You must be logged into a mod account to run this command.";
                    Clients.Caller.addMessage(not_mod);
                    Clients.Caller.addSimpleMessage(not_mod.Nick + " " + not_mod.Message);
                }
                handled = true;
            }

            if (handled == false)
            {
                // Call the addMessage method on all the clients in the room
                Clients.Group(current_user.Room).addMessage(chat);

                string simple_chat = chat.Nick + " " + chat.Message;
                Clients.Group(current_user.Room).addSimpleMessage(simple_chat);

                Messages.Add(chat);
            }


            // every message, update the database
            UpdateDatabase();

            //TimeSpan uptime = DateTime.Now - Started;
            //Clients.Group("admins").addMessage(String.Format("{0} active users. {1} max users. uptime is {2}", Visitors.Count, Max, uptime.Hours + " hours, " + uptime.Minutes + " minutes."));



        }

        public void RequestSimpleBacklog(string message)
        {

            // get the current user
            ChatVisitor current_user = Visitors[Context.ConnectionId];

            List<ChatMessage> chat_backlog = new List<ChatMessage>();
            StringBuilder simple_chat_backlog = new StringBuilder();

            chat_backlog = (from m in Messages where m.Room.Equals(current_user.Room) orderby m.ChatDate descending select m).Take(200).ToList();
            chat_backlog.Reverse();

            foreach (var c in chat_backlog)
            {
                simple_chat_backlog.AppendLine(c.Nick + " " + c.Message);
            }

            Clients.Caller.addSimpleBacklog(simple_chat_backlog.ToString());

        }
    }
}