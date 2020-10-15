using IdentityModel;
using IdentityServer4.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

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
