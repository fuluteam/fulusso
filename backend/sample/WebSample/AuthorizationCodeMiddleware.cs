using System;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using Fulu.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;

namespace WebSample
{
    public class AuthorizationCodeMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly IAuthorizeTokenClient _authorizeTokenClient;
        public AuthorizationCodeMiddleware(RequestDelegate next, IAuthorizeTokenClient authorizeTokenClient)
        {
            _next = next;
            _authorizeTokenClient = authorizeTokenClient;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            var page = context.Request.Path.Value;
            if (page == "/authcode")
            {
                //根据code获取token
                var request = context.Request;
                var host = $"{request.Scheme}://{request.Host}";
                var code = request.Query["code"];
                var state = request.Query["state"];
                var returnUrl = request.Query["return_url"];
                var redirectUri = HttpUtility.UrlEncode($"{host}/authcode?return_url={returnUrl}");
                var result = await _authorizeTokenClient.GetToken(code, state, redirectUri);
                if (!string.IsNullOrEmpty(result.result.AccessToken))
                {
                    context.Response.Cookies.Append("jwt", result.result.AccessToken, new CookieOptions { Expires = DateTimeOffset.Now.AddHours(2) });
                    context.Response.Redirect(returnUrl);
                }
                return;
            }
            if (context.Request.Cookies.TryGetValue("jwt", out string token))
            {
                context.Request.Headers.Add("Authorization", $"Bearer {token}");
            }
            await _next(context);
        }
    }
}