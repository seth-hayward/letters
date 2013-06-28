using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace letterstocrushes.Core.Interfaces
{
    public interface IQueryVotes
    {
        int Vote(int letter_id, string user_ip);
    }
}
