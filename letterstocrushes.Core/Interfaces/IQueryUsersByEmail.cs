using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using letterstocrushes.Core.Model;

namespace letterstocrushes.Core.Interfaces
{
    public interface IQueryUsersByEmail
    {
        User GetUserByEmail(string email);
        User GetUserByGuid(Guid guid);
        PasswordChange AddLostPassword(string user_guid);
        PasswordChange GetLostPassword(string guid);
    }
}
