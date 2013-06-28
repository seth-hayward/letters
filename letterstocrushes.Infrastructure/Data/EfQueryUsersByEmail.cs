using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using letterstocrushes.Core.Interfaces;
using letterstocrushes.Core.Model;
using AutoMapper;

namespace letterstocrushes.Infrastructure.Data
{
    // Ef for EntityFramework
    public class EfQueryUsersByEmail : IQueryUsersByEmail
    {

        private db_mssql _db_mssql;
        private db_mysql _db_mysql;

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

        public EfQueryUsersByEmail()
        {
            Mapper.CreateMap<aspnet_Users, User>();
            Mapper.CreateMap<passwordchange, PasswordChange>();
        }

        public Core.Model.User GetUserByEmail(string email)
        {
            aspnet_Users user = (from m in db_mssql.aspnet_Users
                    where m.UserName == email
                    select m)
                    .FirstOrDefault();

            return Mapper.Map<aspnet_Users, User>(user);
        }

        public Core.Model.User GetUserByGuid(Guid guid)
        {
            aspnet_Users user = (from m in db_mssql.aspnet_Users
                    where m.UserId.Equals(guid)
                    select m)
                    .FirstOrDefault();

            return Mapper.Map<aspnet_Users, User>(user);
        }

        public Core.Model.PasswordChange AddLostPassword(string user_guid)
        {
            Infrastructure.passwordchange lost_password = new Infrastructure.passwordchange();
            lost_password.cguid = System.Guid.NewGuid().ToString();
            lost_password.userguid = user_guid;
            lost_password.created = DateTime.Now;

            db_mssql.passwordchanges.Add(lost_password);
            db_mssql.SaveChanges();

            return Mapper.Map<passwordchange, PasswordChange>(lost_password);
        }

        public Core.Model.PasswordChange GetLostPassword(string guid)
        {
            Infrastructure.passwordchange lost_password = (from l in db_mssql.passwordchanges
                    where l.cguid == guid
                    select l)
                    .FirstOrDefault();

            return Mapper.Map<passwordchange, PasswordChange>(lost_password);
        }
    }
}
