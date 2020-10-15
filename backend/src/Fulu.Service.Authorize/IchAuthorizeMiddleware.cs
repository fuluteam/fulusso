using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;

namespace Microsoft.AspNetCore.Builder
{
    public class IchAuthorizeMiddleware
    {
        private readonly RequestDelegate _next;
        protected IOptionsMonitor<JwtBearerOptions> _optionsMonitor;
        public IchAuthorizeMiddleware(RequestDelegate next, IOptionsMonitor<JwtBearerOptions> optionsMonitor)
        {
            _next = next;
            _optionsMonitor = optionsMonitor;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            if (context.Request.Path.Value.ToLower() == "/refresh_auth_config")
            {
                var op = _optionsMonitor.Get("Bearer");
                op.ConfigurationManager.RequestRefresh();
            }
            await _next.Invoke(context);
        }
    }
}