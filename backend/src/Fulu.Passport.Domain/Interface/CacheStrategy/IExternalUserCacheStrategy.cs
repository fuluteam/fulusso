using System.Collections.Generic;
using System.Threading.Tasks;
using Fulu.AutoDI;
using Fulu.Passport.Domain.Entities;
using Fulu.WebAPI.Abstractions;

namespace Fulu.Passport.Domain.Interface.CacheStrategy
{
    public interface IExternalUserCacheStrategy : IScopedAutoDIable
    {
        Task<ActionObjectResult> BindExternalUser(int clientId, string userId, string providerKey,
            string loginProvider, string nickname);

        Task UnBindExternalUser(int clientId, string userId, string loginProvider);

        Task<ExternalUserEntity> GetExternalUser(int clientId, string providerKey);

        Task<List<ExternalUserEntity>> GetExternalUsers(int clientId, string userId);
    }
}
