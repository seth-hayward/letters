using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using letterstocrushes.Core.Interfaces;
using AutoMapper;
using letterstocrushes.Core.Model;

namespace letterstocrushes.Infrastructure.Data
{
    public class EfQueryEdits : IQueryEdits
    {

        public db_mssql _db_mssql;
        public db_mysql _db_mysql;
        public db_mssql db_mssql
        {
            get
            {
                if (_db_mssql == null)
                {
                    _db_mssql = new db_mssql();
                }

                return _db_mssql;
            }
            set
            {
                _db_mssql = value;
            }
        }
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

        public EfQueryEdits()        
        {
            Mapper.CreateMap<edit, Edit>();
        }

        public List<Core.Model.Edit> getEdits(int page, int _pagesize)
        {
            List<edit> results = new List<edit>();

            letter last = (from m in db_mysql.letters orderby m.Id descending select m).First();

            var query = (from l in db_mssql.edits orderby l.ID descending select l);
            results = query.Skip((page - 1) * _pagesize).Take(_pagesize).ToList();

            return Mapper.Map<List<edit>, List<Edit>>(results);
        }

        public int getEditCount()
        {
            return (from m in db_mssql.edits select m).Count();
        }

    }
}
