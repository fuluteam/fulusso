using System.Threading.Tasks;
using System.Web;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.Extensions.Configuration;

namespace WebSample
{
    public static class MiddlewareExtensions
    {
        public static IApplicationBuilder UseJwtAuthorize(this IApplicationBuilder builder)
        {
            builder.UseMiddleware<AuthorizationCodeMiddleware>();
            builder.UseAuthentication();
            return builder.UseMiddleware<JwtAuthorizeMiddleware>();
        }
    }

    public class JwtAuthorizeMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly IConfiguration _configuration;

        public JwtAuthorizeMiddleware(RequestDelegate next, IConfiguration configuration)
        {
            _next = next;
            _configuration = configuration;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            if (!context.User.Identity.IsAuthenticated)
            {
                var clientId = _configuration["AppSettings:ClientId"];
                var request = context.Request;
                var host = $"{request.Scheme}://{request.Host}";
                var url = request.GetDisplayUrl();
                var redirectUrl = HttpUtility.UrlEncode($"{host}/authcode?return_url={url}");
                context.Response.Redirect($"http://localhost:5000/connect/authorize?client_id={clientId}&redirect_uri={redirectUrl}&response_type=code&scope=api&state=STATE");
                return;
            }
            await _next(context);
        }
    }
}