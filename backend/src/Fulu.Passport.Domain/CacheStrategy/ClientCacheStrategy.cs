using System;
using System.Threading.Tasks;
using FuLu.Passport.Domain.Entities;
using Fulu.Passport.Domain.Interface.CacheStrategy;
using FuLu.Passport.Domain.Interface.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Redis;

namespace Fulu.Passport.Domain.CacheStrategy
{
    public class ClientCacheStrategy : IClientCacheStrategy
    {
        private readonly IRedisCache _redisCache;
        private readonly IClientRepository _clientRepository;

        public ClientCacheStrategy(IClientRepository clientRepository, IRedisCache redisCache)
        {
            _clientRepository = clientRepository;
            _redisCache = redisCache;
        }

        private string GetCacheKey(int clientId)
        {
            return $"ClientId:{clientId}";
        }

        public async Task<ClientEntity> GetClientByIdAsync(int clientId)
        {
            var key = GetCacheKey(clientId);
            var clientEntity = await _redisCache.GetAsync<ClientEntity>(key);
            if (clientEntity != null)
            {
                return clientEntity.ClientId == -1 ? null : clientEntity;
            }

            var expiry = TimeSpan.FromSeconds(5);
            var wait = TimeSpan.FromSeconds(5);
            var retry = TimeSpan.FromSeconds(1);

            using (var redLock = await _redisCache.CreateLockAsync(key, expiry, wait, retry))
            {
                if (!redLock.IsAcquired)
                    return null;

                clientEntity = await _redisCache.GetAsync<ClientEntity>(key);
                if (clientEntity != null && clientEntity.ClientId != -1)
                {
                    return clientEntity;
                }
                clientEntity = await _clientRepository.TableNoTracking.FirstOrDefaultAsync(c => c.ClientId == clientId);

                if (clientEntity != null)
                {
                    await _redisCache.AddAsync(key, clientEntity);
                }
                else
                {
                    //防止不存在的clientId频繁访问数据库
                    await _redisCache.AddAsync(key, new ClientEntity { ClientId = -1 }, expiry: TimeSpan.FromMinutes(15));
                }
            }

            return clientEntity;
        }

        public async Task ClearCacheByIdAsync(int clientId)
        {
            var key = GetCacheKey(clientId);
            await _redisCache.KeyDeleteAsync(key);
        }
    }
}
