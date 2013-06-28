using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace letterstocrushes.Core.Model
{
    public class PasswordChange
    {
        public int changeid { get; set; }
        public string cguid { get; set; }
        public string userguid { get; set; }
        public System.DateTime created { get; set; }
    }
}
