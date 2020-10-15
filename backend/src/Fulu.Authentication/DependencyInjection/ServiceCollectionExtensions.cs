using Fulu.Authentication;
using Fulu.Authentication.Options;
using Polly;
using Polly.Timeout;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class ServiceCollectionExtensions
    {
        public static void AddAuthorizeTokenClient(this IServiceCollection services, Action<AuthorizeTokenOptions> configure, Uri baseUri)
        {
            if (services == null)
                throw new ArgumentNullException(nameof(services));
            if (configure == null)
                throw new ArgumentNullException(nameof(configure));

            services.Configure(configure);

            var retryPolicy = Policy<HttpResponseMessage>.Handle<HttpRequestException>().Or<TimeoutRejectedException>().RetryAsync(3);
            var timeoutPolicy = Policy.TimeoutAsync<HttpResponseMessage>(2);
            services.AddFuluHttpClient<IAuthorizeTokenClient, AuthorizeTokenClient>("authorize_client", baseUri).AddPolicyHandler(Policy.WrapAsync(retryPolicy, timeoutPolicy));
        }
    }
}
