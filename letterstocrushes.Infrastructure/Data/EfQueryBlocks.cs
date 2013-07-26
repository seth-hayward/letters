using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using letterstocrushes.Core.Interfaces;

namespace letterstocrushes.Infrastructure.Data
{
    public class EfQueryBlocks : IQueryBlocks
    {

        private db_mysql _db_mysql;
        public db_mysql db_mysql
        {
            get
            {
                if (_db_mysql == null)
                {
                    _db_mysql = new db_mysql();
                }

                return _db_mysql;
            }
            set
            {
                _db_mysql = value;
            }
        }

        public void Add(int type, string value, int what)
        {
            throw new NotImplementedException();
        }

        public List<Core.Model.Block> getBlocks(int type, int what)
        {
            throw new NotImplementedException();
        }
    }
}
