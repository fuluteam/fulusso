using System;
using System.Security.Claims;
using System.Threading.Tasks;
using Fulu.Core.Extensions;
using Fulu.Passport.Domain.Interface.Repositories;
using FuLu.IdentityServer.Stores;
using IdentityModel;
using IdentityServer4.Models;

namespace IdentityServer4.Stores
{
    public class ClientStore : IClientStore
    {
        private readonly IClientInCacheRepository _clientInCacheRepository;

        public ClientStore(IClientInCacheRepository clientInCacheRepository)
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
                    GrantType.ResourceOwnerPassword
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
