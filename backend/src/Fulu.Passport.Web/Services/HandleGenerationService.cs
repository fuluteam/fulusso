using IdentityModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

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
