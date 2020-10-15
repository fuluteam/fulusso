using Fulu.AspNetCore;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class CookiePolicyExtensions
    {
        public static IServiceCollection AddCookiePolicy(this IServiceCollection services)
        {
            return services.Configure<CookiePolicyOptions>(options =>
             {
                 //options.CheckConsentNeeded = context => true;
                 //options.MinimumSameSitePolicy = SameSiteMode.None;
                 options.MinimumSameSitePolicy = SameSiteMode.Unspecified;
                 options.OnAppendCookie = cookieContext =>
                     CheckSameSite(cookieContext.Context, cookieContext.CookieOptions);
                 options.OnDeleteCookie = cookieContext =>
                     CheckSameSite(cookieContext.Context, cookieContext.CookieOptions);
             });

            void CheckSameSite(HttpContext httpContext, CookieOptions options)
            {
                if (options.SameSite == SameSiteMode.None)
                {
                    //var userAgent = httpContext.Request.Headers["User-Agent"].ToString();
                    //if (MyUserAgentDetectionLib.DisallowsSameSiteNone(userAgent))
                    //{
#if NETCOREAPP2_1
                        options.SameSite = SameSiteMode.Lax;
#elif NETCOREAPP3_1
                    options.SameSite = SameSiteMode.Unspecified;
#endif

                    //}
                }
            }
        }
    }
}
