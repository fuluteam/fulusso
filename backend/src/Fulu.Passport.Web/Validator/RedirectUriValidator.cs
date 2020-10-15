using IdentityServer4.Models;
using IdentityServer4.Validation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace IdentityServer4.Validation
{
    public class RedirectUriValidator : IRedirectUriValidator
    {
        public Task<bool> IsRedirectUriValidAsync(string requestedUri, Client client)
        {
            if (client.RedirectUris == null || !client.RedirectUris.Any())
            {
                return Task.FromResult(false);
            }
            var uri = new Uri(requestedUri);
            var host = uri.Host;
            return Task.FromResult(client.RedirectUris.Any(x => x.Contains(host)));
        }

        public Task<bool> IsPostLogoutRedirectUriValidAsync(string requestedUri, Client client)
        {
            return Task.FromResult(true);
        }
    }
}
