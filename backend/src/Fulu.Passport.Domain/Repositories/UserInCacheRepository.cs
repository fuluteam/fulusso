using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Fulu.Passport.Domain.Interface.Repositories;
using FuLu.Passport.Domain.Entities;
using FuLu.Passport.Domain.Interface.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Redis;

namespace Fulu.Passport.Domain.Repositories
{
    public class UserInCacheRepository : IUserInCacheRepository
    {
        private readonly IRedisCache _redisCache;
        private readonly IUserRepository _userRepository;
        public UserInCacheRepository(IRedisCache redisCache, IUserRepository userRepository)
        {
            _redisCache = redisCache;
            _userRepository = userRepository;
        }
        public async Task<UserEntity> GetUserByPhoneAsync(string phone)
        {
            var userEntity = new UserEntity { Phone = phone };

            userEntity = await _redisCache.ObjectGetAsync(userEntity);
            if (userEntity != null)
            {
                return userEntity;
            }

            userEntity = await _userRepository.TableNoTracking.FirstOrDefaultAsync(c => c.Phone == phone);
            if (userEntity != null)
            {
                await _redisCache.ObjectAddAsync(userEntity, TimeSpan.FromDays(15));
            }
            return userEntity;
        }


    }
}
