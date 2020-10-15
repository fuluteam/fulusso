using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Microsoft.AspNetCore.Builder
{
    public static class MiddlewareExtensions
    {
        public static IApplicationBuilder UseHealthCheck(this IApplicationBuilder builder)
        {
            return builder.UseRouter(router =>
            {
                router.MapGet("hc", ctx =>
                {
                    ctx.Response.StatusCode = 204;
                    return Task.CompletedTask;
                });
            });
        }
    }
}
