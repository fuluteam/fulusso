using ICH.TransferJob;

namespace Microsoft.Extensions.DependencyInjection
{

    public static class InjectionServiceCollectionExtensions
    {
        public static IServiceCollection AddTransferJob(this IServiceCollection services)
        {
            services.AddSingleton<IBackgroundRunService, BackgroundRunService>();
            services.AddHostedService<TransferJobHostedService>();
            return services;
        }

    }
}