using System;
using System.Threading.Tasks;
using IdentityServer4.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace Fulu.Passport.Web.Endpoints
{
    public class WeChatEndpoint : IEndpointHandler
    {
        private readonly ILogger<WeChatEndpoint> _logger;
        private readonly IServiceProvider _serviceProvider;
        public WeChatEndpoint(ILogger<WeChatEndpoint> logger, IServiceProvider serviceProvider)
        {
            _logger = logger;
            _serviceProvider = serviceProvider;
        }

        public async Task<IEndpointResult> ProcessAsync(HttpContext context)
        {
            if (!HttpMethods.IsGet(context.Request.Method))
            {
                _logger.LogWarning("Invalid HTTP request for thirdparty endpoint");
                throw new Exception();
            }
            var values = context.Request.Query.AsNameValueCollection();
            return new WeChatResult(values, _serviceProvider);
        }
    }
}