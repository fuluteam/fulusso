using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Fulu.Passport.Domain.Entities;
using FuLu.Passport.Domain.Entities;
using Fulu.Passport.Domain.Interface.Repositories;
using Fulu.Passport.Domain.Interface.Services;
using Fulu.WebAPI.Abstractions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Redis;

namespace Fulu.Passport.Domain.Services
{
    public class ExternalUserService : IExternalUserService
    {
        private readonly IRedisCache _redisCache;
        private readonly IExternalUserRepository _externalUserRepository;
        private readonly IUnitOfWork _unitOfWork;
        public ExternalUserService(IRedisCache redisCache, IExternalUserRepository externalUserRepository, IUnitOfWork unitOfWork)
        {
            _redisCache = redisCache;
            _externalUserRepository = externalUserRepository;
            _unitOfWork = unitOfWork;
        }

        /// <summary>
        /// 绑定第三方账号
        /// </summary>
        public async Task<ActionObjectResult> BindExternalUser(int clientId, string userId, string providerKey, string loginProvider, string nickname)
        {
            var externalUsers = await GetExternalUsers(clientId, userId);
            var externalUser = externalUsers.FirstOrDefault(c => c.LoginProvider == loginProvider && c.ClientId == clientId);

            if (externalUser != null)
            {
                return ActionObject.Ok(20026, "通行证账号已绑定过该类型账号");
            }

            externalUser = await GetExternalUser(clientId, providerKey);

            if (!string.IsNullOrEmpty(externalUser?.UserId))
            {
                return ActionObject.Ok(20027, "此用户已绑定过通行证账号");
            }

            var externalUserEntity = new ExternalUserEntity()
            {
                UserId = userId,
                ProviderKey = providerKey,
                LoginProvider = loginProvider,
                Nickname = nickname,
                ClientId = clientId,
                CreateDate = DateTime.Now,
            };

            await _externalUserRepository.InsertAsync(externalUserEntity);
            await _unitOfWork.SaveChangesAsync();

            await _redisCache.KeyDeleteAsync(GetExternalUsersRedisKey(clientId, userId));
            await _redisCache.KeyDeleteAsync(GetExternalUserRedisKey(clientId, providerKey));
            return ActionObject.Ok();
        }

        /// <summary>
        /// 解绑第三方账号
        /// </summary>
        public async Task UnBindExternalUser(int clientId, string userId, string loginProvider)
        {
            var externalUsers = await GetExternalUsers(clientId, userId);
            var externalUser = externalUsers.FirstOrDefault(c => c.ClientId == clientId && c.LoginProvider == loginProvider);
            if (externalUser == null) return;

            _externalUserRepository.Delete(c => c.ClientId == clientId && c.ProviderKey == externalUser.ProviderKey && c.UserId == userId);
            await _unitOfWork.SaveChangesAsync();

            await _redisCache.KeyDeleteAsync(GetExternalUsersRedisKey(clientId, userId));
        }

        private string GetExternalUsersRedisKey(int clientId, string userId)
        {
            return $"{clientId}:{userId}";
        }

        private string GetExternalUserRedisKey(int clientId, string providerKey)
        {
            return $"{clientId}:{providerKey}";
        }

        /// <summary>
        /// 根据第三方OpenId查询绑定信息
        /// </summary>
        public async Task<ExternalUserEntity> GetExternalUser(int clientId, string providerKey, bool rebuild = false)
        {
            var key = GetExternalUserRedisKey(clientId, providerKey);
            ExternalUserEntity externalUserEntity;
            if (!rebuild)
            {
                externalUserEntity = await _redisCache.GetAsync<ExternalUserEntity>(key);
                if (externalUserEntity != null) return externalUserEntity;
            }

            externalUserEntity = await _externalUserRepository.TableNoTracking.FirstOrDefaultAsync(
               c => c.ProviderKey == providerKey && c.ClientId == clientId);
            if (null == externalUserEntity) return null;

            await _redisCache.AddAsync(key, externalUserEntity, TimeSpan.FromDays(15));
            return externalUserEntity;
        }

        /// <summary>
        /// 根据用户Id查询绑定信息
        /// </summary>
        public async Task<List<ExternalUserEntity>> GetExternalUsers(int clientId, string userId, bool rebuild = false)
        {
            var key = GetExternalUsersRedisKey(clientId, userId);
            List<ExternalUserEntity> externalUsers;
            if (!rebuild)
            {
                externalUsers = await _redisCache.GetAsync<List<ExternalUserEntity>>(key);
                if (externalUsers != null && externalUsers.Any()) return externalUsers;
            }

            externalUsers = await _externalUserRepository.TableNoTracking.Where(c => c.UserId == userId && c.ClientId == clientId).ToListAsync();

            if (externalUsers.Any())
                await _redisCache.AddAsync(key, externalUsers, TimeSpan.FromDays(15));

            return externalUsers;
        }
    }
}
