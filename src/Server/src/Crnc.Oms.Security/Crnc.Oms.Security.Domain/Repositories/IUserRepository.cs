using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Crnc.Oms.Security.Domain.SeedWork;
using Crnc.Oms.Security.Domain.Aggregates.Users;
using Crnc.Oms.Security.Domain.Dto;

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
        /// <param name="cancellationToken">CancellationToken</param>
        /// <returns></returns>
        Task<User> FindByLoginAsync(string login, CancellationToken cancellationToken = default);

        /// <summary>
        /// Find all users info by filter
        /// </summary>
        /// <returns></returns>
        Task<IEnumerable<UserItemDto>> FindByFilterAsync(UserFilterDto dto,CancellationToken cancellationToken = default);
        
        /// <summary>
        /// Find short users info by filter
        /// </summary>
        /// <returns></returns>
        Task<IEnumerable<UserShortInfoItemDto>> FindByFilterShortInfoAsync(UserFilterDto dto,CancellationToken cancellationToken = default);
        
        /// <summary>
        /// Get all roles of application
        /// </summary>
        /// <returns></returns>
        Task<IEnumerable<Role>> GetRolesAsync(CancellationToken cancellationToken = default);
        
        /// <summary>
        /// Get role by id
        /// </summary>
        /// <returns></returns>
        Task<Role> GetRoleByIdAsync(Guid id, CancellationToken cancellationToken = default);
    }
}
