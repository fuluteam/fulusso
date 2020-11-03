using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using IdentityServer4.Models;
using IdentityServer4.Stores;
using Microsoft.Extensions.Caching.Redis;
using StackExchange.Redis;

namespace FuLu.IdentityServer.Stores
{
    public class PersistedGrantStore : IPersistedGrantStore
    {
        private const string _dateFormatString = "yyyy-MM-dd HH:mm:ss";
        private readonly IRedisCache _redisCache;
        public PersistedGrantStore(IRedisCache redisCache)
        {
            _redisCache = redisCache;
        }

        public async Task<IEnumerable<PersistedGrant>> GetAllAsync(string subjectId)
        {
            if (string.IsNullOrWhiteSpace(subjectId))
                return new List<PersistedGrant>();

            var db = await _redisCache.GetDatabaseAsync();

            var keys = await db.ListRangeAsync(subjectId);

            var list = new List<PersistedGrant>();
            foreach (string key in keys)
            {
                var items = await db.HashGetAllAsync(key);
                list.Add(GetPersistedGrant(items));
            }

            return list;
        }

        public async Task<PersistedGrant> GetAsync(string key)
        {
            var db = await _redisCache.GetDatabaseAsync();
            var items = await db.HashGetAllAsync(key);

            return GetPersistedGrant(items);
        }

        public async Task RemoveAllAsync(string subjectId, string clientId)
        {
            if (string.IsNullOrEmpty(subjectId) || string.IsNullOrEmpty(clientId))
                return;
            var db = await _redisCache.GetDatabaseAsync();
            await db.KeyDeleteAsync($"{subjectId}:{clientId}");
        }

        public async Task RemoveAllAsync(string subjectId, string clientId, string type)
        {
            if (string.IsNullOrEmpty(subjectId) || string.IsNullOrEmpty(clientId) || string.IsNullOrEmpty(type))
                return;
            var db = await _redisCache.GetDatabaseAsync();
            await db.KeyDeleteAsync($"{subjectId}:{clientId}:{type}");
        }

        public async Task RemoveAsync(string key)
        {
            var db = await _redisCache.GetDatabaseAsync();
            await db.KeyDeleteAsync(key);
        }

        public async Task StoreAsync(PersistedGrant grant)
        {
            //var expiresIn = grant.Expiration - DateTimeOffset.UtcNow;
            var db = await _redisCache.GetDatabaseAsync();

            var trans = db.CreateTransaction();

            var expiry = grant.Expiration.Value.ToLocalTime();

            db.HashSetAsync(grant.Key, GetHashEntries(grant));
            db.KeyExpireAsync(grant.Key, expiry);


            if (!string.IsNullOrEmpty(grant.SubjectId))
            {
                db.ListLeftPushAsync(grant.SubjectId, grant.Key);
                db.KeyExpireAsync(grant.SubjectId, expiry);

                var key1 = $"{grant.SubjectId}:{grant.ClientId}";
                db.ListLeftPushAsync(key1, grant.Key);
                db.KeyExpireAsync(key1, expiry);

                var key2 = $"{grant.SubjectId}:{grant.ClientId}:{grant.Type}";
                db.ListLeftPushAsync(key2, grant.Key);
                db.KeyExpireAsync(key2, expiry);
            }

            await trans.ExecuteAsync();
        }

        private HashEntry[] GetHashEntries(PersistedGrant grant)
        {
            return new[]
            {
                new HashEntry("key", grant.Key),
                new HashEntry("type", grant.Type),
                new HashEntry("sub", grant.SubjectId??""),
                new HashEntry("client", grant.ClientId),
                new HashEntry("create", grant.CreationTime.ToString(_dateFormatString)),
                new HashEntry("expire", grant.Expiration == null ? default(DateTime).ToString(_dateFormatString) : grant.Expiration.Value.ToString(_dateFormatString)),
                new HashEntry("data", grant.Data),
            };
        }

        private PersistedGrant GetPersistedGrant(HashEntry[] entries)
        {
            if (entries.Length != 7)
                return null;

            var grant = new PersistedGrant();
            foreach (var item in entries)
            {
                if (item.Name == "key")
                {
                    grant.Key = item.Value;
                }
                if (item.Name == "type")
                {
                    grant.Type = item.Value;
                }
                if (item.Name == "sub")
                {
                    grant.SubjectId = item.Value;
                }
                if (item.Name == "client")
                {
                    grant.ClientId = item.Value;
                }
                if (item.Name == "create")
                {
                    grant.CreationTime = DateTime.Parse(item.Value);
                }
                if (item.Name == "expire")
                {
                    grant.Expiration = DateTime.Parse(item.Value);
                    if (grant.Expiration.Value == default)
                    {
                        grant.Expiration = null;
                    }
                }
                if (item.Name == "data")
                {
                    grant.Data = item.Value;
                }
            }

            return grant;
        }
    }
}
