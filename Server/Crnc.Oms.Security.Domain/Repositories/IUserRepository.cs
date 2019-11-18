using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Crnc.Oms.Domain.SeedWork;
using Crnc.Oms.Security.Domain.Aggregates.Users;

namespace Crnc.Oms.Security.Domain.Repositories
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
        Task<User> FindByLoginAsync(string login, CancellationToken cancellationToken = default);

        /// <summary>
        /// Get all roles of application
        /// </summary>
        /// <returns></returns>
        Task<IEnumerable<Role>> GetRolesAsync(CancellationToken cancellationToken = default);
    }
}
