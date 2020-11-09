using System;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using Fulu.AutoDI;
using Fulu.Core.Extensions;
using FuLu.IdentityServer;
using FuLu.IdentityServer.Stores;
using Fulu.Passport.Domain;
using Fulu.Passport.Domain.Interface;
using FuLu.Passport.Domain.Interface;
using Fulu.Passport.Domain.Options;
using FuLu.Passport.Domain.Options;
using Fulu.Passport.Domain.Services;
using Fulu.Passport.Web.Validator;
using IdentityServer4;
using IdentityServer4.Configuration;
using IdentityServer4.Services;
using IdentityServer4.Stores;
using IdentityServer4.Validation;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Org.BouncyCastle.Utilities.Encoders;
using StackExchange.Redis;

namespace Fulu.Passport.Web
{
    public class Startup
    {
        // This method gets called by the runtime. Use this method to add services to the container.
        // For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940
        private IConfiguration Configuration;
        public IHostEnvironment Env { get; }

        public Startup(IConfiguration configuration, IHostEnvironment environment)
        {
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
            Configuration = configuration;
            Env = environment;
        }


        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {

            //AppSettings
            var appSettings = services.ConfigureOptions<AppSettings>(Configuration.GetSection("AppSettings"));

            var userInteractionOptions = services.ConfigureOptions<UserInteractionOptions>(Configuration.GetSection("UserInteraction"));

            var endpoints = services.ConfigureOptions<Endpoints>(Configuration.GetSection("Endpoints"));

            services.ConfigureOptions<MapOptions>(Configuration.GetSection("MapOptions"));

            services.ConfigureOptions<SmsOptions>(Configuration.GetSection("Sms"));

            //滑块验证码
            services.ConfigureOptions<CaptchaOptions>(Configuration.GetSection("Captcha"));

            //Redis缓存配置
            var redisOptions = services.ConfigureOptions<RedisOptions>(Configuration.GetSection("Redis"));

            //初始化配置
            ConfigureInit(services, appSettings, endpoints);

            ConfigureIdentityServer(services, userInteractionOptions, appSettings);

            //添加缓存
            ConfigureCache(services, redisOptions);

            //数据库配置
            var connectionStrings = services.ConfigureOptions<ConnectionStrings>(Configuration.GetSection("ConnectionStrings"));
            //添加EF Core
            ConfigureEntityFrameworkCore(services, connectionStrings);

        }

        private void ConfigureInit(IServiceCollection services, AppSettings appSettings, Endpoints endpoints)
        {
            services.AddControllers().AddControllersAsServices().AddNewtonsoftJson(opt => opt.SerializerSettings.DateFormatString = "yyyy-MM-dd HH:mm:ss");

            services.ConfigureApiBehavior();

            services.AddCookiePolicy();

            services.AddCors(opt => opt.AddDefaultPolicy(builder =>
            {
                //builder.WithOrigins(Configuration["CorsOrigins"].Split(','));
                builder.SetIsOriginAllowed(x => true);
                builder.AllowAnyHeader();
                builder.AllowAnyMethod();
                builder.AllowCredentials();
            }));

            services.AddHealthChecks();

            //Swagger
            if (!Env.IsProduction())
            {
                services.AddDocument<Startup>(appSettings.ClientName, "web");
            }

            services.AutoDI(typeof(FuluDbContext).Assembly);

            services.AddAuthorizeTokenClient(o =>
            {
                o.ClientId = appSettings.ClientId.ToString();
                o.ClientSecret = appSettings.ClientSecret;
            }, new Uri(endpoints.Authority));

            services.AddFuluHttpClient<IExternalClient, ExternalClient>("external");

            services.AddFuluHttpClient<IPassportClient, PassportClient>("passport");

            //配置获取客户端ip
            services.Configure<ForwardedHeadersOptions>(opt =>
            {
                opt.ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto;
                opt.KnownNetworks.Clear();
                opt.KnownProxies.Clear();
            });

            services.AddTransferJob();

            services.AddAuthentication()
                .AddWeChat(o =>
            {
                o.SignInScheme = IdentityServerConstants.ExternalCookieAuthenticationScheme;
                o.ClientId = Configuration["ExternalWeChat:AppId"];
                o.ClientSecret = Configuration["ExternalWeChat:Secret"];
            })
                .AddDingTalk(o =>
            {
                o.SignInScheme = IdentityServerConstants.ExternalCookieAuthenticationScheme;
                o.ClientId = Configuration["ExternalDingTalk:AppId"];
                o.ClientSecret = Configuration["ExternalDingTalk:Secret"];
            });
        }

        private void ConfigureIdentityServer(IServiceCollection services, UserInteractionOptions userInteractionOptions, AppSettings appSettings)
        {

            var identityServerBuilder = services.AddIdentityServer(o =>
            {
                o.Authentication = new AuthenticationOptions
                {
                    CookieLifetime = TimeSpan.FromSeconds(7200),
                    CookieSlidingExpiration = false
                };
                o.InputLengthRestrictions = new InputLengthRestrictions
                {
                    Password = 256,
                    TokenHandle = 200
                };
                o.IssuerUri = "http://localhost:80";
                o.AccessTokenJwtType = "JWT";
                o.UserInteraction = userInteractionOptions;
            });

            var cookieDomain = Configuration["CookieDomain"];
            if (!cookieDomain.IsEmpty())
            {
                services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
                    .AddCookie(CookieAuthenticationDefaults.AuthenticationScheme,
                        opt => { opt.Cookie.Domain = Configuration["CookieDomain"]; });
                services.AddAuthorization(opt =>
                {
                    opt.AddPolicy("TwoFactorEnabled", x => x.RequireClaim("amr", "mfa"));
                });
            }

            identityServerBuilder.AddSigningCredential(new X509Certificate2(Hex.Decode(appSettings.X509RawCertData), appSettings.X509CertPwd));
            identityServerBuilder.AddClientStore<ClientStore>();
            identityServerBuilder.AddResourceStore<ResourceStore>();
            identityServerBuilder.AddPersistedGrantStore<PersistedGrantStore>();
            identityServerBuilder.AddResourceOwnerValidator<ResourceOwnerPasswordValidator>();
            identityServerBuilder.AddRedirectUriValidator<RedirectUriValidator>();
            identityServerBuilder.AddCustomAuthorizeRequestValidator<CustomAuthorizeRequestValidator>();
            identityServerBuilder.AddExtensionGrantValidator<SmsGrantValidator>();
            identityServerBuilder.AddExtensionGrantValidator<ExternalGrantValidator>();

            services.AddTransient<IHandleGenerationService, CustomHandleGenerationService>();
            services.AddTransient<IAuthorizationCodeStore, AuthorizationCodeStore>();
        }

        /// <summary>
        /// King
        /// </summary>
        private void ConfigureEntityFrameworkCore(IServiceCollection services, ConnectionStrings connectionStrings)
        {
            //添加King
            services.AddEntityFrameworkCore<FuluDbContext>(opt => opt.UseMySql(connectionStrings.MySql));
        }
        /// <summary>
        /// Cache
        /// </summary>
        private void ConfigureCache(IServiceCollection services, RedisOptions redisOptions)
        {
            //添加缓存
            services.AddRedisCache(x =>
            {
                var options = ConfigurationOptions.Parse(redisOptions.ConnectionString);
                options.DefaultDatabase = redisOptions.Database;
                x.ConfigurationOptions = options;
                x.InstanceName = redisOptions.InstanceName;
            });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            if (!Env.IsProduction())
            {
                app.UseDocument();
            }
            app.UseHealthChecks("/hc");
            app.UseForwardedHeaders();
            app.UseCors();
            app.UseCookiePolicy();
            app.UseMiddleware<CompatibilityPassportMiddleware>();
            app.UseIdentityServer();
            app.UseRouting();
            app.UseAuthentication();
            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllerRoute("default", "{controller=Home}/{action=Index}/{id?}");
            });
        }

    }
}
