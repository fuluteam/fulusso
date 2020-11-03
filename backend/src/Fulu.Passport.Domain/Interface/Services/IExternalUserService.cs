using Fulu.AutoDI;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Fulu.Passport.Domain.Entities;
using Fulu.WebAPI.Abstractions;

namespace Fulu.Passport.Domain.Interface.Services
{
    public interface IExternalUserService : IScopedAutoDIable
    {
        Task<ActionObjectResult> BindExternalUser(int clientId, string userId, string providerKey,
            string loginProvider, string nickname);

        Task UnBindExternalUser(int clientId, string userId, string loginProvider);

        Task<ExternalUserEntity> GetExternalUser(int clientId, string providerKey, bool rebuild = false);

        Task<List<ExternalUserEntity>> GetExternalUsers(int clientId, string userId, bool rebuild = false);
    }
}
