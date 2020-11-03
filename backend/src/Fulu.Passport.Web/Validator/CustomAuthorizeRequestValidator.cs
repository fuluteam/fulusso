using System;
using System.Threading.Tasks;

namespace IdentityServer4.Validation
{
    public class CustomAuthorizeRequestValidator : ICustomAuthorizeRequestValidator
    {
        public Task ValidateAsync(CustomAuthorizeRequestValidationContext context)
        {
            var redirectUri = new Uri(context.Result.ValidatedRequest.RedirectUri);
            if (!string.IsNullOrWhiteSpace(redirectUri.Query))
            {
                context.Result.ValidatedRequest.RedirectUri = redirectUri.AbsoluteUri;
            }
            return Task.CompletedTask;
        }
    }
}
