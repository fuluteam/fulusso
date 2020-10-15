using IdentityServer4.Configuration;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using System.Web;

namespace FuLu.IdentityServer
{
    public class CompatibilityPassportMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly UserInteractionOptions _userInteractionOptions;

        public CompatibilityPassportMiddleware(RequestDelegate next, UserInteractionOptions userInteractionOptions)
        {
            _next = next;
            _userInteractionOptions = userInteractionOptions;
        }

        public async Task Invoke(HttpContext context)
        {
            if (context.Request.Path.HasValue)
            {
                var path = context.Request.Path.Value;
                if (path.StartsWith("/oauth", true, CultureInfo.CurrentCulture))
                {
                    context.Request.Path = path.Replace("/oauth", "/connect", true, CultureInfo.CurrentCulture);
                }
                else if (path.Equals("/user/login", StringComparison.CurrentCultureIgnoreCase))
                {
                    var returnUrl = context.Request.Query["ReturnUrl"].ToString();
                    if (!string.IsNullOrEmpty(returnUrl))
                    {
                        var domain = $"{context.Request.Scheme}://{context.Request.Host}";
                        if (!returnUrl.StartsWith("http"))
                        {
                            returnUrl = $"{domain}/{returnUrl.TrimStart('/')}";
                        }
                    }
                    context.Response.Redirect($"{_userInteractionOptions.LoginUrl}?ReturnUrl={HttpUtility.UrlEncode(returnUrl)}");
                    return;
                }
            }

            await _next(context);
        }
    }
}
