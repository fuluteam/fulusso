using System;
using Microsoft.Extensions.Caching.Redis.Snowflake;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class SnowflakeDependencyInjection
    {
        public static IServiceCollection AddSnowflake(this IServiceCollection service, Action<SnowflakeOption> option)
        {
            service.Configure(option);
            service.AddSingleton<ISnowflakeIdMaker, SnowflakeIdMaker>();
            var opt = new SnowflakeOption();
            option(opt);
            if (opt.EnableAutoWorkId)
            {
                service.AddHostedService<SnowflakeBackgroundServices>();
            }
            return service;
        }
    }
}