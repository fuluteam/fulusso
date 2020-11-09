using System.Security.Claims;
using System.Threading.Tasks;
using Fulu.Core.Extensions;
using FuLu.IdentityServer.Stores;
using Fulu.Passport.Domain.Interface.CacheStrategy;
using Fulu.Passport.Domain.Interface.Repositories;
using IdentityModel;
using IdentityServer4.Models;

namespace IdentityServer4.Stores
{
    public class ClientStore : IClientStore
    {
        private readonly IClientCacheStrategy _clientInCacheRepository;

        public ClientStore(IClientCacheStrategy clientInCacheRepository)
        {
            _clientInCacheRepository = clientInCacheRepository;
        }
        public async Task<Client> FindClientByIdAsync(string clientId)
        {

            var clientEntity = await _clientInCacheRepository.GetClientByIdAsync(clientId.ToInt32());
            if (clientEntity == null)
            {
                return null;
            }
            return new Client
            {
                ClientId = clientId,
                AllowedScopes = new[] { "api", "get_user_info" },
                ClientSecrets = new[] { new Secret(clientEntity.ClientSecret.Sha256()) },
                AllowedGrantTypes = new[]
                {
                    GrantType.AuthorizationCode,
                    GrantType.ClientCredentials,
                    GrantType.ResourceOwnerPassword,
                    CustomGrantType.External,
                    CustomGrantType.Sms
                },
                AllowOfflineAccess = false,
                RedirectUris = string.IsNullOrWhiteSpace(clientEntity.RedirectUri) ? null : clientEntity.RedirectUri.Split(';'),
                RequireConsent = false,
                AccessTokenType = AccessTokenType.Jwt,
                AccessTokenLifetime = 7200,
                ClientClaimsPrefix = "",
                Claims = new[] { new Claim(JwtClaimTypes.Role, "Client"), new Claim(ClaimTypes.Role, "Client") }
            };
        }


    }
}
