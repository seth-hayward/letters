using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace letterstocrushes.Core.Model
{
    public class Bookmark
    {
        public int BookmarkId { get; set; }
        public System.DateTime AddDate { get; set; }
        public System.DateTime RevisedDate { get; set; }
        public int Visible { get; set; }
        public int LetterId { get; set; }
        public int Type { get; set; }
        public string UserGuid { get; set; }
    }
}
