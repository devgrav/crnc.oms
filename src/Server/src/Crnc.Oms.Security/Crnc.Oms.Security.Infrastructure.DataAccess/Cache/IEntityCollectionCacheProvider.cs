using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Crnc.Oms.Security.Domain.SeedWork;
using Microsoft.Extensions.Caching.Distributed;

namespace Crnc.Oms.Security.Infrastructure.DataAccess.Cache
{
    public interface IEntityCollectionCacheProvider<T>
    {
        Task<IEnumerable<T>> GetAsync(string key);

        Task RefreshAsync(string key);
    }
}