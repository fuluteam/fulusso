using System.Security.Claims;
using System.Threading.Tasks;
using IdentityModel;
using IdentityServer4.Configuration;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace IdentityServer4.Services
{
    public class FuluUserSession : DefaultUserSession
    {
        public FuluUserSession(IHttpContextAccessor httpContextAccessor, IAuthenticationSchemeProvider schemes,
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
