using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using letterstocrushes.Core.Interfaces;
using letterstocrushes.Core.Model;

namespace letterstocrushes.Core.Services
{
    public class EditService
    {

        private readonly IQueryEdits _queryEdits;
        public EditService(IQueryEdits queryEdits)
        {
            _queryEdits = queryEdits;
        }

        public List<Edit> getEdits(int page, int _pagesize, string time_zone)
        {
            return fixList(_queryEdits.getEdits(page, _pagesize), time_zone);
        }

        public int getEditCount()
        {
            return _queryEdits.getEditCount();
        }

        public List<Edit> fixList(List<Edit> edits, string time_zone)
        {

            List<Edit> result = new List<Edit>();

            if (time_zone != null)
            {

                int offset = 0;
                offset = Convert.ToInt32(time_zone);

                foreach (Edit edit in edits)
                {
                    Edit temp_edit = edit.Clone();
                    DateTime postdate = temp_edit.editDate;
                    postdate = postdate.AddHours(offset * -1);
                    temp_edit.editDate = postdate;
                    result.Add(temp_edit);

                    // only providing htmldiffs on letters that were not hidden...
                    if (temp_edit.editComment.ToLower().Contains("hidden") == false & temp_edit.editComment.ToLower().Contains("queue") == false & temp_edit.editComment.ToLower().Contains("publish") == false)
                    {
                        HtmlDiff html_diff = new HtmlDiff(temp_edit.previousLetter, temp_edit.newLetter);
                        temp_edit.newLetter = html_diff.Build();
                    }
                }

            }

            return result;

        }


    }
}
