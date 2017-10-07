using Crnc.Oms.Domain.Aggregates.Users;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Crnc.Oms.Domain.IRepositories
{
    /// <summary>
    /// Interface of users repository
    /// </summary>
    public interface IUserRepository
        :IRepository<User>
    {
        /// <summary>
        /// Find user by username/login
        /// </summary>
        /// <param name="login">login</param>
        /// <returns></returns>
        User FindByLogin(string login);
    }
}
