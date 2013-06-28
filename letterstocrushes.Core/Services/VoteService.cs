using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using letterstocrushes.Core.Model;
using letterstocrushes.Core.Interfaces;

namespace letterstocrushes.Core.Services
{
    public class VoteService
    {

        private readonly IQueryVotes _queryVotes;
        public VoteService(IQueryVotes queryVotes)
        {
            _queryVotes = queryVotes;
        }

        public int Vote(int letter_id, string user_ip)
        {
            return _queryVotes.Vote(letter_id, user_ip);
        }

    }
}
