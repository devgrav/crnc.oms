using System;
using System.Linq;
using System.Linq.Expressions;
using Crnc.Oms.Security.Domain.Aggregates.Users;

namespace Crnc.Oms.Security.Infrastructure.DataAccess.QueryExtensions
{
    public static class RoleQueries
    {
        public static Expression<Func<Role, bool>> ById(Guid id)
        {
            return u => u.Id == id;
        }
    }
}