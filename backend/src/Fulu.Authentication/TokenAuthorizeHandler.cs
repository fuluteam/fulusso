using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace Microsoft.AspNetCore.Authorization
{
    public class TokenAuthorizeHandler : AuthorizationHandler<AuthorizeRequirement>
    {
        private readonly ILogger<TokenAuthorizeHandler> _logger;
        public TokenAuthorizeHandler(ILogger<TokenAuthorizeHandler> logger)
        {
            _logger = logger;
        }

        protected override Task HandleRequirementAsync(AuthorizationHandlerContext context, AuthorizeRequirement requirement)
        {
            #region 验证令牌
            if (!context.User.Identity.IsAuthenticated)
            {
                _logger.LogDebug("the token is not authenticated");
                context.Fail();
                return Task.CompletedTask;
            }

            #endregion

            if (!requirement.AllowClientToken && context.User.HasClaim(ClaimTypes.Role,ClaimRoles.Client))
            {//不允许客户端令牌访问&&访问令牌是客户端令牌
                _logger.LogDebug("client token forbidden");
                context.Fail();
                return Task.CompletedTask;
            }

            if (!requirement.AllowUserToken && context.User.HasClaim(ClaimTypes.Role, ClaimRoles.User))
            {//不允许用户令牌访问&&访问令牌是用户端令牌
                _logger.LogDebug("user token forbidden");
                context.Fail();
                return Task.CompletedTask;
            }

            context.Succeed(requirement);
            return Task.CompletedTask;
        }
    }
}
