using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using letterstocrushes.Core.Interfaces;
using letterstocrushes.Core.Model;

namespace letterstocrushes.Core.Services
{
    public class UserService
    {

        private readonly IQueryUsersByEmail _queryUsersByEmail;
        public UserService(IQueryUsersByEmail queryUsersByEmail)
        {
            _queryUsersByEmail = queryUsersByEmail;
        }

        public User getUserByEmail(string email)
        {
            return _queryUsersByEmail.GetUserByEmail(email);
        }

        public User getUserByGuid(Guid guid)
        {
            return _queryUsersByEmail.GetUserByGuid(guid);
        }

        public PasswordChange AddLostPassword(string user_guid)
        {
            return _queryUsersByEmail.AddLostPassword(user_guid);
        }

        public PasswordChange GetLostPassword(string guid)
        {
            return _queryUsersByEmail.GetLostPassword(guid);
        }

    }
}
