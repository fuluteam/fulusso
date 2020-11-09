using System.Threading.Tasks;
using Fulu.AutoDI;
using FuLu.Passport.Domain.Entities;

namespace Fulu.Passport.Domain.Interface.CacheStrategy
{
    public interface IClientCacheStrategy : IScopedAutoDIable
    {
        Task<ClientEntity> GetClientByIdAsync(int clientId);
        Task ClearCacheByIdAsync(int clientId);
    }
}
