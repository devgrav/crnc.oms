using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Crnc.Oms.Security.Domain.SeedWork;
using Crnc.Oms.Security.Infrastructure.DataAccess.Cache;
using Crnc.Oms.Security.Infrastructure.DataAccess.Exceptions;
using Microsoft.Extensions.Caching.Distributed;
using MongoDB.Driver;

namespace Crnc.Oms.Security.Infrastructure.DataAccess.Cache
{
    public class MongoEntityCollectionCacheProvider<T>
        : IEntityCollectionCacheProvider<T>
    {
        private readonly IDistributedCache _cache;
        private readonly MongoDataContext _mongoDataContext;

        public MongoEntityCollectionCacheProvider(IDistributedCache cache, MongoDataContext mongoDataContext)
        {
            _cache = cache;
            _mongoDataContext = mongoDataContext;
        }
        
        public async Task<IEnumerable<T>> GetAsync(string key)
        {
            var entityData =  await _cache.GetAsync(key);
            IEnumerable<T> collection = null;
            if (entityData != null && entityData.Any())
            {
                collection = JsonSerializer.Deserialize<IEnumerable<T>>(entityData);
                return collection;
            }

            await RefreshAsync(key);

            collection = await _mongoDataContext.Collection<T>(key).AsQueryable().ToListAsync();
            
            return collection;
        }

        public async Task RefreshAsync(string key)
        {
            var collection = await _mongoDataContext.Collection<T>(key).AsQueryable().ToListAsync();

            await SetAsync(collection, key);
        }

        private async Task SetAsync(IEnumerable<T> entities, string key)
        {
            var json = JsonSerializer.Serialize(entities);

            await _cache.SetAsync(key, Encoding.UTF8.GetBytes(json));
        }
    }
}