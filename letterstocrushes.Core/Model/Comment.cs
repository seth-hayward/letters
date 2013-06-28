using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace letterstocrushes.Core.Model
{
    public class Comment
    {
        public int Id { get; set; }
        public string commentMessage { get; set; }
        public string commenterName { get; set; }
        public int letterId { get; set; }
        public bool sendEmail { get; set; }
        public System.DateTime commentDate { get; set; }
        public int hearts { get; set; }
        public string commenterEmail { get; set; }
        public string commenterGuid { get; set; }
        public Nullable<int> level { get; set; }
    }
}
