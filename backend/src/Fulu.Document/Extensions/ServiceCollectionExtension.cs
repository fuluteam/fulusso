using ICH.Document.Filters;
using Swashbuckle.AspNetCore.Swagger;
using Swashbuckle.AspNetCore.SwaggerGen;
using System;
using System.Reflection;
using Microsoft.OpenApi.Models;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class ServiceCollectionExtension
    {
        public static IServiceCollection AddDocument<T>(this IServiceCollection services, string title, string description = null, string version = "v3") where T : class
        {
            var info = new OpenApiInfo
            {
                Title = title,
                Version = version,
                Description = description,
                //TermsOfService = Assembly.GetEntryAssembly().ManifestModule.Name.Replace(".dll", "")
            };
            return services.AddDocument<T>(m => m.SwaggerDoc(version, info));
        }

        public static IServiceCollection AddDocument<T>(this IServiceCollection services, Action<SwaggerGenOptions> setupAction) where T : class
        {
            services.AddSwaggerGen(c =>
            {
                setupAction?.Invoke(c);
                c.OperationFilter<AuthResponsesOperationFilter>();
                c.AddOAuth2Definition();
                c.IncludeXmlComments();
            });
            return services;
        }
    }
}

