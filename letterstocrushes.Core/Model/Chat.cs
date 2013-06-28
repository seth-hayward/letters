using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace letterstocrushes.Core.Model
{
    public class Chat
    {
        public int id { get; set; }
        public string Message { get; set; }
        public string Nick { get; set; }
        public System.DateTime ChatDate { get; set; }
        public string IP { get; set; }
        public string Room { get; set; }
    }
}
