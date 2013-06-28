using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using letterstocrushes.Core.Model;
using letterstocrushes.Core.Interfaces;

namespace letterstocrushes.Core.Services
{
    public class QueueService
    {

        private readonly IQueryQueue _queryQueue;
        public QueueService(IQueryQueue queryQueue)
        {
            _queryQueue = queryQueue;
        }

        public List<Letter> getQueuedLetters()
        {
            return _queryQueue.getQueuedLetters();
        }

        public void Dequeue(int letter_id)
        {
            _queryQueue.Dequeue(letter_id);
        }

        public void Queue(int letter_id, string user_name)
        {
            _queryQueue.Queue(letter_id, user_name);
        }

        public Letter PublishQueue() {
            return _queryQueue.PublishQueue();
        }

    }
}
