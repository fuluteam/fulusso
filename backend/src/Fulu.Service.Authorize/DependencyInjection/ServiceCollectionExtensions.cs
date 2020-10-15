using System;
using System.Text;
using Microsoft.AspNetCore.Mvc;
using Polly;
using System.Net.Http;
using Polly.Timeout;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Options;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using IdentityModel;
using System.Security.Claims;
using Microsoft.IdentityModel.Logging;
using Fulu.Authentication;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class ServiceCollectionExtensions
    {

        public static IServiceCollection AddAuthentication(this IServiceCollection services, Action<AuthorizeRequirement> configure)
        {
            if (services == null)
                throw new ArgumentNullException(nameof(services));
            if (configure == null)
                throw new ArgumentNullException(nameof(configure));

            var req = new AuthorizeRequirement
            {
                AllowClientToken = true,
                AllowUserToken = true
            };

            configure(req);

            if (string.IsNullOrWhiteSpace(req.Authority))
            {
                throw new ArgumentNullException(nameof(req.Authority), "此参数用于获取认证授权中心配置的信息，请务必设置为认证授权的终结点");
            }

            services.AddAuthentication(x =>
            {
                x.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                x.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            }).AddJwtBearer(o =>
            {
                IdentityModelEventSource.ShowPII = true;
                o.Authority = req.Authority;
                o.RequireHttpsMetadata = false;

                o.TokenValidationParameters = new TokenValidationParameters
                {
                    NameClaimType = JwtClaimTypes.Name,
                    RoleClaimType = ClaimTypes.Role,
                    ValidIssuer = req.ValidIssuer,
                    ValidAudiences = req.ValidAudiences,
                    /***********************************TokenValidationParameters的参数默认值***********************************/
                    // RequireSignedTokens = true,
                    // SaveSigninToken = false,
                    // ValidateActor = false,
                    // 将下面两个参数设置为false，可以不验证Issuer和Audience，但是不建议这样做。
                    ValidateAudience = req.ValidateAudience,
                    ValidateIssuer = req.ValidateIssuer,
                    // ValidateIssuerSigningKey = false,
                    // 是否要求Token的Claims中必须包含Expires
                    RequireExpirationTime = true,
                    // 允许的服务器时间偏移量
                    ClockSkew = TimeSpan.FromSeconds(300),
                    // 是否验证Token有效期，使用当前时间与Token的Claims中的NotBefore和Expires对比
                    ValidateLifetime = true
                };
            });
            services.AddAuthorization(options =>
            {
                options.AddPolicy("Basic",
                    policy =>
                    {
                        policy.AddAuthenticationSchemes(JwtBearerDefaults.AuthenticationScheme);
                        policy.RequireAuthenticatedUser();
                        policy.Requirements.Add(req);
                    });
            });
            services.AddSingleton<IAuthorizationHandler, TokenAuthorizeHandler>();  // 注入拦截对象

            return services;
        }

        public static IServiceCollection AddServiceAuthorize(this IServiceCollection services, Action<ServiceAuthorizeOptions> configure)
        {
            if (services == null)
                throw new ArgumentNullException(nameof(services));
            if (configure == null)
                throw new ArgumentNullException(nameof(configure));

            services.Configure(configure);

            var options = new ServiceAuthorizeOptions();

            configure(options);

            services.AddAuthentication(o =>
            {
                o.AllowClientToken = options.AllowClientToken;
                o.AllowUserToken = options.AllowUserToken;
                o.ValidAudiences = options.ValidAudiences;
                o.Authority = options.Authority;
                o.ValidIssuer = options.ValidIssuer;
                o.ValidateAudience = options.ValidateAudience;
                o.ValidateIssuer = options.ValidateIssuer;
            });

            services.AddAuthorizeTokenClient(o =>
            {
                o.ClientId = options.ClientId;
                o.ClientSecret = options.ClientSecret;
            },new Uri(options.Authority));

            services.Configure<MvcOptions>(o => o.Filters.Add<ServiceAuthorizeFilter>());

            services.AddSingleton<ServiceAuthorizeFilter>();

            return services;
        }


    }
}
