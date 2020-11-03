using System.Threading.Tasks;
using IdentityModel;
using IdentityServer4.Services;

namespace FuLu.IdentityServer.Stores
{
    public class CustomHandleGenerationService : IHandleGenerationService
    {
        public Task<string> GenerateAsync(int length = 32)
        {
            return Task.FromResult(CryptoRandom.CreateUniqueId(64));
        }
    }
}
