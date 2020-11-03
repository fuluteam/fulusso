using System.Threading.Tasks;
using IdentityModel;

namespace IdentityServer4.Services
{
    public class HandleGenerationService : IHandleGenerationService
    {
        public Task<string> GenerateAsync(int length = 32)
        {
            return Task.FromResult(CryptoRandom.CreateUniqueId(64));
        }
    }
}
