using IdentityServer4.Extensions;
using IdentityServer4.Models;
using IdentityServer4.Services;
using IdentityServer4.Stores;
using IdentityServer4.Stores.Serialization;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace IdentityServer4.Stores
{
    public class AuthorizationCodeStore : DefaultGrantStore<AuthorizationCode>, IAuthorizationCodeStore
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="DefaultAuthorizationCodeStore"/> class.
        /// </summary>
        /// <param name="store">The store.</param>
        /// <param name="serializer">The serializer.</param>
        /// <param name="handleGenerationService">The handle generation service.</param>
        /// <param name="logger">The logger.</param>
        public AuthorizationCodeStore(
            IPersistedGrantStore store,
            IPersistentGrantSerializer serializer,
            IHandleGenerationService handleGenerationService,
            ILogger<DefaultAuthorizationCodeStore> logger)
            : base(IdentityServerConstants.PersistedGrantTypes.AuthorizationCode, store, serializer, handleGenerationService, logger)
        {
        }

        /// <summary>
        /// Stores the authorization code asynchronous.
        /// </summary>
        /// <param name="code">The code.</param>
        /// <returns></returns>
        public Task<string> StoreAuthorizationCodeAsync(AuthorizationCode code)
        {
            return CreateItemAsync(code, code.ClientId, code.Subject.GetSubjectId(), code.CreationTime, code.Lifetime);
        }

        /// <summary>
        /// Gets the authorization code asynchronous.
        /// </summary>
        /// <param name="code">The code.</param>
        /// <returns></returns>
        public async Task<AuthorizationCode> GetAuthorizationCodeAsync(string code)
        {
            var store = await GetItemAsync(code);
            if (store == null) 
                return null;
            var redirectUri = new Uri(store.RedirectUri);
            if (!string.IsNullOrWhiteSpace(redirectUri.Query))
            {
                store.RedirectUri = new Uri(store.RedirectUri).ToString();
            }
            return store;
        }

        /// <summary>
        /// Removes the authorization code asynchronous.
        /// </summary>
        /// <param name="code">The code.</param>
        /// <returns></returns>
        public Task RemoveAuthorizationCodeAsync(string code)
        {
            return RemoveItemAsync(code);
        }
    }
}
