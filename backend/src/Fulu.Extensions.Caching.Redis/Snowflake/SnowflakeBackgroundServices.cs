using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;

namespace Microsoft.Extensions.Caching.Redis.Snowflake
{
    public class SnowflakeBackgroundServices : BackgroundService
    {
        private readonly IRedisCache _cache;
        private readonly ISnowflakeIdMaker _idMaker;

        public SnowflakeBackgroundServices(IRedisCache cache, ISnowflakeIdMaker idMaker)
        {
            _cache = cache;
            _idMaker = idMaker;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            if (!stoppingToken.IsCancellationRequested)
            {
                while (true)
                {
                    //每5分钟更新在线的key的score
                    await _cache.SortedSetAddAsync(_idMaker.GetUserInKey(), _idMaker.WorkId().ToString(), DateTime.Now.ToUnixTimestamp(), token: stoppingToken);
                    await Task.Delay(280000, stoppingToken);
                }

            }
        }
    }
}