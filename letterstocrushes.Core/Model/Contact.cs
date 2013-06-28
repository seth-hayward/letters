using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace letterstocrushes.Core.Model
{
    public class Contact
    {

        private string l_Email;
        private string l_Message;

        public string Email
        {
            get { return l_Email; }
            set { l_Email = value; }
        }

        public string Message
        {
            get { return l_Message; }
            set { l_Message = value; }
        }

    }
}
