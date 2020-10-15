using System;
using System.Collections.Generic;
using System.Text;
using Fulu.Google.Authenticator;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class ServiceCollectionExtension
    {
        public static IServiceCollection AddGoogleAuthenticator(this IServiceCollection services)
        {
            services.AddSingleton<ITwoFactorAuthenticator, TwoFactorAuthenticator>();
            return services;
        }
    }
}

