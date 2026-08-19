using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Crnc.Oms.Security.Domain.Aggregates.Users;
using Crnc.Oms.Security.Domain.SeedWork;
using Crnc.Oms.Security.Infrastructure.DataAccess.Cache;
using Crnc.Oms.Security.Infrastructure.DataAccess.Exceptions;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Caching.Memory;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Driver;
using MongoDB.Driver.Linq;

namespace Crnc.Oms.Security.Infrastructure.DataAccess.Cache
{
    public class MongoInMemoryEntityCollectionCacheProvider<T>
        : IEntityCollectionCacheProvider<T>
    {
        private readonly IMemoryCache _cache;
        private readonly MongoDataContext _mongoDataContext;

        public MongoInMemoryEntityCollectionCacheProvider(IMemoryCache cache, MongoDataContext mongoDataContext)
        {
            _cache = cache;
            _mongoDataContext = mongoDataContext;
        }
        
        public async Task<IEnumerable<T>> GetAsync(string key)
        {
            var entities = _cache.Get<IEnumerable<T>>(key);
            IEnumerable<T> collection = null;
            if (entities != null && entities.Any())
            {
                return entities;
            }

            await RefreshAsync(key);

            collection = await _mongoDataContext.Collection<T>(key).AsQueryable().ToListAsync();
            
            return collection;
        }

        public async Task RefreshAsync(string key)
        {
            var collection = await _mongoDataContext.Collection<T>(key).AsQueryable().ToListAsync();

            _cache.Set(key, collection);
        }
    }
}