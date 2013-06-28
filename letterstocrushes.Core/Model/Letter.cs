using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace letterstocrushes.Core.Model
{
    public class Letter : ICloneable
    {
        public int Id { get; set; }
        public string letterMessage { get; set; }
        public string letterTags { get; set; }
        public System.DateTime letterPostDate { get; set; }
        public Nullable<int> letterUp { get; set; }
        public Nullable<int> letterLevel { get; set; }
        public string letterLanguage { get; set; }
        public string senderIP { get; set; }
        public string senderCountry { get; set; }
        public string senderRegion { get; set; }
        public string senderCity { get; set; }
        public Nullable<int> letterComments { get; set; }

        object ICloneable.Clone()
        {
            return this.Clone();
        }

        public Letter Clone()
        {
            return (Letter)this.MemberwiseClone();
        }
    }
}
