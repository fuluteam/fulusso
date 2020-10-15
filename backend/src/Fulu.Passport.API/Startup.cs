using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AutoMapper;
using Fulu.AutoDI;
using Fulu.Passport.Domain;
using Fulu.Passport.Domain.Interface;
using Fulu.Passport.Domain.Options;
using Fulu.Passport.Domain.Services;
using FuLu.Passport.Domain.Interface;
using FuLu.Passport.Domain.Options;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using StackExchange.Redis;

namespace Fulu.Passport.API
{
    /// <summary>
    /// 
    /// </summary>
    public class Startup
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="configuration"></param>
        /// <param name="environment"></param>
        public Startup(IConfiguration configuration, IHostEnvironment environment)
        {
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
            Configuration = configuration;
            Env = environment;
        }
        /// <summary>
        /// 
        /// </summary>
        public IHostEnvironment Env { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public IConfiguration Configuration { get; }

        /// <summary>
        /// This method gets called by the runtime. Use this method to add services to the container.
        /// </summary>
        /// <param name="services"></param>
        public void ConfigureServices(IServiceCollection services)
        {
            //AppSettings
            var appSettings = services.ConfigureOptions<AppSettings>(Configuration.GetSection("AppSettings"));

            var endpoints = services.ConfigureOptions<Endpoints>(Configuration.GetSection("Endpoints"));

            services.ConfigureOptions<MapOptions>(Configuration.GetSection("MapOptions"));

            services.ConfigureOptions<SmsOptions>(Configuration.GetSection("Sms"));

            //ª¨øÈ—È÷§¬Î
            services.ConfigureOptions<CaptchaOptions>(Configuration.GetSection("Captcha"));

            //Redisª∫¥Ê≈‰÷√
            var redisOptions = services.ConfigureOptions<RedisOptions>(Configuration.GetSection("Redis"));

            // ˝æ›ø‚≈‰÷√
            var connectionStrings = services.ConfigureOptions<ConnectionStrings>(Configuration.GetSection("ConnectionStrings"));

            //≥ı ºªØ≈‰÷√
            ConfigureInit(services, appSettings);

            //ÃÌº”ª∫¥Ê
            ConfigureCache(services, redisOptions);

            //ÃÌº”EF Core
            ConfigureEntityFrameworkCore(services, connectionStrings);

            //ÃÌº”»œ÷§ ⁄»®
            ConfigureAuthorize(services, endpoints, appSettings);

            if (!Env.IsProduction())
            {
                foreach (System.Collections.Generic.KeyValuePair<string, string> item in Configuration.AsEnumerable())
                {
                    Console.WriteLine($"{item.Key} : {item.Value}");
                }
            }

        }

        private void ConfigureInit(IServiceCollection services, AppSettings appSettings)
        {
            services.AddControllers().AddControllersAsServices().AddNewtonsoftJson(opt => opt.SerializerSettings.DateFormatString = "yyyy-MM-dd HH:mm:ss");

            services.ConfigureApiBehavior();

            services.AddCors(opt => opt.AddDefaultPolicy(builder =>
            {
                builder.WithOrigins(Configuration["CorsOrigins"].Split(','));
                builder.SetIsOriginAllowed(x => true);
                builder.AllowAnyHeader();
                builder.AllowAnyMethod();
                builder.AllowCredentials();
            }));

            services.AddHealthChecks();

            services.AddAutoMapper(config =>
            {
                config.ValidateInlineMaps = false;
            });

            services.AddHttpContextAccessor();

            services.AutoDI(typeof(FuluDbContext).Assembly);

            //Swagger
            if (!Env.IsProduction())
            {
                services.AddDocument<Startup>(appSettings.ClientName, "api");
            }

            services.AddTransferJob();

            services.AddFuluHttpClient<IExternalClient, ExternalClient>("external-api", null);

            services.AddFuluHttpClient<IPassportClient, PassportClient>("passport-api", null);
        }

        private void ConfigureAuthorize(IServiceCollection services, Endpoints endpoints, AppSettings appSettings)
        {
            services.AddServiceAuthorize(o =>
            {
                o.AllowClientToken = true;
                o.AllowUserToken = true;
                o.OnClientValidate = false;
                o.Authority = endpoints.Authority;
                o.ValidateAudience = false;
                o.ClientId = appSettings.ClientId.ToString();
                o.ClientSecret = appSettings.ClientSecret;
            });
        }

        /// <summary>
        /// King
        /// </summary>
        private void ConfigureEntityFrameworkCore(IServiceCollection services, ConnectionStrings connectionStrings)
        {
            //ÃÌº”King
            services.AddEntityFrameworkCore<FuluDbContext>(opt => opt.UseMySql(connectionStrings.MySql));
        }

        /// <summary>
        /// Cache
        /// </summary>
        private void ConfigureCache(IServiceCollection services, RedisOptions redisOptions)
        {
            //ÃÌº”ª∫¥Ê
            services.AddRedisCache(x =>
            {
                var options = ConfigurationOptions.Parse(redisOptions.ConnectionString);
                options.DefaultDatabase = redisOptions.Database;
                x.ConfigurationOptions = options;
                x.InstanceName = redisOptions.InstanceName;
            });
        }

        /// <summary>
        /// This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        /// </summary>
        /// <param name="app"></param>
        /// <param name="env"></param>
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
            app.UseRouting();
            app.UseAuthentication();
            app.UseAuthorization();
            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllerRoute("default", "{controller=Home}/{action=Index}/{id?}");
            });
        }
    }
}
