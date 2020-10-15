using IdentityModel;
using IdentityServer4.Configuration;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace IdentityServer4.Services
{
    public class UserSession : DefaultUserSession
    {
        public UserSession(IHttpContextAccessor httpContextAccessor, IAuthenticationSchemeProvider schemes,
            IAuthenticationHandlerProvider handlers, IdentityServerOptions options, ISystemClock clock,
            ILogger<IUserSession> logger) : base(httpContextAccessor, schemes, handlers, options, clock, logger)
        {
        }

        public override async Task<ClaimsPrincipal> GetUserAsync()
        {
            var principal = await base.GetUserAsync();
            if (principal != null)
            {
                var amr = principal.FindFirstValue(JwtClaimTypes.AuthenticationMethod);
                if (!amr.Contains("mfa"))
                {
                    return null;
                }
            }

            return principal;
        }
    }
}
