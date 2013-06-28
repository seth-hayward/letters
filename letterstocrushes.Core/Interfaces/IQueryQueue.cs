using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using letterstocrushes.Core.Model;
using letterstocrushes.Core.Interfaces;

namespace letterstocrushes.Core.Interfaces
{
    public interface IQueryQueue
    {
        List<Letter> getQueuedLetters();
        void Dequeue(int letter_id);
        void Queue(int letter_id, string user_name);
        Letter PublishQueue();
    }
}
