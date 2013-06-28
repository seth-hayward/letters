using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace letterstocrushes.Core.Model
{
    public class Edit : ICloneable
    {
        public int ID { get; set; }
        public int letterID { get; set; }
        public string previousLetter { get; set; }
        public string newLetter { get; set; }
        public System.DateTime editDate { get; set; }
        public string status { get; set; }
        public string editor { get; set; }
        public string editComment { get; set; }

        object ICloneable.Clone()
        {
            return this.Clone();
        }
        public Edit Clone()
        {
            return (Edit)this.MemberwiseClone();
        }

    }
}
