using System;
using System.Net.Http;
using Polly;
using Polly.Timeout;
using Polly.Registry;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class ServiceCollectionExtensions
    {

        public static IHttpClientBuilder AddFuluHttpClient<TClient, TImplementation>(this IServiceCollection services, string name)
            where TClient : class
            where TImplementation : class, TClient
        {
            return services.AddFuluHttpClient<TClient, TImplementation>(name, null);
        }

        public static IHttpClientBuilder AddFuluHttpClient<TClient, TImplementation>(this IServiceCollection services, string name, Uri endpoint)
        where TClient : class
        where TImplementation : class, TClient
        {
            IHttpClientBuilder clientBuilder;
            if (endpoint != null)
            {
                clientBuilder = services.AddHttpClient<TClient, TImplementation>(name, options =>
                {
                    options.BaseAddress = endpoint;
                });
            }
            else
            {
                clientBuilder = services.AddHttpClient<TClient, TImplementation>(name);
            }
            return clientBuilder;
        }

       

        /// <summary>
        /// 通过内置Polly支持重试、超时、熔断等
        /// </summary>
        public static IHttpClientBuilder WithDefaultPolicy(this IHttpClientBuilder builder)
        {
            builder.Services.AddDefaultPolicy();
            return builder.AddPolicyHandlerFromRegistry("default");
        }

        public static IPolicyRegistry<string> AddDefaultPolicy(this IServiceCollection services)
        {
            return services.AddPollyPolicy(3, TimeSpan.FromSeconds(8));
        }

        public static IPolicyRegistry<string> AddPollyPolicy(this IServiceCollection services,
            int retryCount,
            TimeSpan timeout)
        {
            var registry = services.AddPolicyRegistry();
            var retryPolicy = Policy<HttpResponseMessage>.Handle<HttpRequestException>()
                .RetryAsync(3);
            var timeoutPolicy = Policy.TimeoutAsync<HttpResponseMessage>(8, TimeoutStrategy.Optimistic);
            registry.Add("default", Policy.WrapAsync(retryPolicy, timeoutPolicy));

            return registry;
        }

        public static IServiceCollection AddCommonPolicy(this IServiceCollection services)
        {
            var registry = services.AddPolicyRegistry();
            var retry = Policy<HttpResponseMessage>.Handle<HttpRequestException>()
                .RetryAsync(3);
            var timeout = Policy.TimeoutAsync<HttpResponseMessage>(8, TimeoutStrategy.Optimistic);
            registry.Add("common", Policy.WrapAsync(retry, timeout));

            return services;
        }
    }
}
