using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace letterstocrushes.Core.Model
{

    public enum blockType
    {
        blockIP = 1,
        blockSubnet = 2,
        blockGUID = 3
    }

    public enum blockWhat
    {
        blockLetter = 1,
        blockComment = 2,
        blockChat = 3
    }

    public class Block
    {
        public int Id { get; set; }
        public Nullable<int> Type { get; set; }
        public string Value { get; set; }
        public Nullable<int> What { get; set; }
    }
}
