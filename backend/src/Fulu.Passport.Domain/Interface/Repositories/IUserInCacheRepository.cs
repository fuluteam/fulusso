using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Fulu.AutoDI;
using FuLu.Passport.Domain.Entities;

namespace Fulu.Passport.Domain.Interface.Repositories
{
    /// <summary>
    /// 
    /// </summary>
    public interface IUserInCacheRepository : IScopedAutoDIable
    {
        Task<UserEntity> GetUserByPhoneAsync(string phone);
    }
}
