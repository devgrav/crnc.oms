using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Crnc.Oms.Domain.SeedWork;
using Crnc.Oms.Security.Domain.Aggregates.Users;

namespace Crnc.Oms.Security.Domain.IRepositories
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

        /// <summary>
        /// Get all roles of application
        /// </summary>
        /// <returns></returns>
        IEnumerable<Role> GetRoles();
    }
}
