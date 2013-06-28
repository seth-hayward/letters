using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using letterstocrushes.Core.Model;

namespace letterstocrushes.Core.Interfaces
{
    public interface IQueryEdits
    {
        List<Edit> getEdits(int page, int _pagesize);
        int getEditCount();
    }
}
