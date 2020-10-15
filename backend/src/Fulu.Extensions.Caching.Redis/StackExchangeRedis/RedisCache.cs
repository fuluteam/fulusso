using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using RedLockNet;
using RedLockNet.SERedis;
using RedLockNet.SERedis.Configuration;
using StackExchange.Redis;

namespace Microsoft.Extensions.Caching.Redis
{
    public class RedisCache : IRedisCache, IDisposable
    {
        // KEYS[1] = = key
        // ARGV[1] = absolute-expiration - ticks as long (-1 for none)
        // ARGV[2] = sliding-expiration - ticks as long (-1 for none)
        // ARGV[3] = relative-expiration (long, in seconds, -1 for none) - Min(absolute-expiration - Now, sliding-expiration)
        // ARGV[4] = data - byte[]
        // this order should not change LUA script depends on it
        private const string SetScript = (@"
                redis.call('HMSET', KEYS[1], 'absexp', ARGV[1], 'sldexp', ARGV[2], 'data', ARGV[4])
                if ARGV[3] ~= '-1' then
                  redis.call('EXPIRE', KEYS[1], ARGV[3])
                end
                return 1");
        private const string AbsoluteExpirationKey = "absexp";
        private const string SlidingExpirationKey = "sldexp";
        private const string DataKey = "data";
        private const long NotPresent = -1;

        private volatile ConnectionMultiplexer _connection;
        private IDatabase _cache;

        private readonly RedisCacheOptions _options;
        private readonly string _instance;

        private readonly SemaphoreSlim _connectionLock = new SemaphoreSlim(initialCount: 1, maxCount: 1);

        private RedLockFactory _redLockFactory;

        public RedisCache(IOptions<RedisCacheOptions> optionsAccessor)
        {
            if (optionsAccessor == null)
            {
                throw new ArgumentNullException(nameof(optionsAccessor));
            }

            _options = optionsAccessor.Value;

            // This allows partitioning a single backend cache for use with multiple apps/services.
            _instance = _options.InstanceName ?? string.Empty;
        }

        public byte[] GetAndRefresh(string key)
        {
            if (key == null)
            {
                throw new ArgumentNullException(nameof(key));
            }

            return GetAndRefresh(GetKeyForRedis(key), getData: true);
        }

        public async Task<byte[]> GetAndRefreshAsync(string key, CancellationToken token = default)
        {
            if (key == null)
            {
                throw new ArgumentNullException(nameof(key));
            }

            token.ThrowIfCancellationRequested();

            return await GetAndRefreshAsync(GetKeyForRedis(key), getData: true, token: token);
        }

        public void ScriptEvaluate(string key, byte[] value, DistributedCacheEntryOptions options)
        {
            if (key == null)
            {
                throw new ArgumentNullException(nameof(key));
            }

            if (value == null)
            {
                throw new ArgumentNullException(nameof(value));
            }

            if (options == null)
            {
                throw new ArgumentNullException(nameof(options));
            }

            Connect();


            var creationTime = DateTimeOffset.UtcNow;

            var absoluteExpiration = GetAbsoluteExpiration(creationTime, options);

            _cache.ScriptEvaluate(SetScript, new RedisKey[] { GetKeyForRedis(key) },
                  new RedisValue[]
                  {
                        absoluteExpiration?.Ticks ?? NotPresent,
                        options.SlidingExpiration?.Ticks ?? NotPresent,
                        GetExpirationInSeconds(creationTime, absoluteExpiration, options) ?? NotPresent,
                        value
                  });
        }

        public async Task ScriptEvaluateAsync(string key, byte[] value, DistributedCacheEntryOptions options, CancellationToken token = default)
        {
            if (key == null)
            {
                throw new ArgumentNullException(nameof(key));
            }

            if (value == null)
            {
                throw new ArgumentNullException(nameof(value));
            }

            if (options == null)
            {
                throw new ArgumentNullException(nameof(options));
            }

            token.ThrowIfCancellationRequested();

            await ConnectAsync(token: token);

            var creationTime = DateTimeOffset.UtcNow;

            var absoluteExpiration = GetAbsoluteExpiration(creationTime, options);

            await _cache.ScriptEvaluateAsync(SetScript, new RedisKey[] { GetKeyForRedis(key) },
                new RedisValue[]
                {
                        absoluteExpiration?.Ticks ?? NotPresent,
                        options.SlidingExpiration?.Ticks ?? NotPresent,
                        GetExpirationInSeconds(creationTime, absoluteExpiration, options) ?? NotPresent,
                        value
                });
        }

        private byte[] GetAndRefresh(string key, bool getData)
        {
            if (key == null)
            {
                throw new ArgumentNullException(nameof(key));
            }

            Connect();

            // This also resets the LRU status as desired.
            // TODO: Can this be done in one operation on the server side? Probably, the trick would just be the DateTimeOffset math.
            RedisValue[] results;
            if (getData)
            {
                results = _cache.HashMemberGet(GetKeyForRedis(key), AbsoluteExpirationKey, SlidingExpirationKey, DataKey);
            }
            else
            {
                results = _cache.HashMemberGet(GetKeyForRedis(key), AbsoluteExpirationKey, SlidingExpirationKey);
            }

            // TODO: Error handling
            if (results.Length >= 2)
            {
                MapMetadata(results, out DateTimeOffset? absExpr, out TimeSpan? sldExpr);
                Refresh(key, absExpr, sldExpr);
            }

            if (results.Length >= 3 && results[2].HasValue)
            {
                return results[2];
            }

            return null;
        }

        private async Task<byte[]> GetAndRefreshAsync(string key, bool getData, CancellationToken token = default)
        {
            if (key == null)
            {
                throw new ArgumentNullException(nameof(key));
            }

            token.ThrowIfCancellationRequested();

            await ConnectAsync(token: token);

            // This also resets the LRU status as desired.
            // TODO: Can this be done in one operation on the server side? Probably, the trick would just be the DateTimeOffset math.
            RedisValue[] results;
            if (getData)
            {
                results = await _cache.HashMemberGetAsync(GetKeyForRedis(key), AbsoluteExpirationKey, SlidingExpirationKey, DataKey);
            }
            else
            {
                results = await _cache.HashMemberGetAsync(GetKeyForRedis(key), AbsoluteExpirationKey, SlidingExpirationKey);
            }

            // TODO: Error handling
            if (results.Length >= 2)
            {
                MapMetadata(results, out DateTimeOffset? absExpr, out TimeSpan? sldExpr);
                await RefreshAsync(GetKeyForRedis(key), absExpr, sldExpr, token);
            }

            if (results.Length >= 3 && results[2].HasValue)
            {
                return results[2];
            }

            return null;
        }

        private void MapMetadata(RedisValue[] results, out DateTimeOffset? absoluteExpiration, out TimeSpan? slidingExpiration)
        {
            absoluteExpiration = null;
            slidingExpiration = null;
            var absoluteExpirationTicks = (long?)results[0];
            if (absoluteExpirationTicks.HasValue && absoluteExpirationTicks.Value != NotPresent)
            {
                absoluteExpiration = new DateTimeOffset(absoluteExpirationTicks.Value, TimeSpan.Zero);
            }
            var slidingExpirationTicks = (long?)results[1];
            if (slidingExpirationTicks.HasValue && slidingExpirationTicks.Value != NotPresent)
            {
                slidingExpiration = new TimeSpan(slidingExpirationTicks.Value);
            }
        }

        private void Refresh(string key, DateTimeOffset? absExpr, TimeSpan? sldExpr)
        {
            if (key == null)
            {
                throw new ArgumentNullException(nameof(key));
            }

            if (sldExpr.HasValue)
            {
                // Note Refresh has no effect if there is just an absolute expiration (or neither).
                TimeSpan? expr;
                if (absExpr.HasValue)
                {
                    var relExpr = absExpr.Value - DateTimeOffset.Now;
                    expr = relExpr <= sldExpr.Value ? relExpr : sldExpr;
                }
                else
                {
                    expr = sldExpr;
                }
                _cache.KeyExpire(GetKeyForRedis(key), expr);
                // TODO: Error handling
            }
        }

        private async Task RefreshAsync(string key, DateTimeOffset? absExpr, TimeSpan? sldExpr, CancellationToken token = default)
        {
            if (key == null)
            {
                throw new ArgumentNullException(nameof(key));
            }

            token.ThrowIfCancellationRequested();
            if (sldExpr.HasValue)
            {

                // Note Refresh has no effect if there is just an absolute expiration (or neither).
                TimeSpan? expr;
                if (absExpr.HasValue)
                {
                    var relExpr = absExpr.Value - DateTimeOffset.Now;
                    expr = relExpr <= sldExpr.Value ? relExpr : sldExpr;
                }
                else
                {
                    expr = sldExpr;
                }
                await _cache.KeyExpireAsync(GetKeyForRedis(key), expr);
                // TODO: Error handling
            }
        }

        private static long? GetExpirationInSeconds(DateTimeOffset creationTime, DateTimeOffset? absoluteExpiration, DistributedCacheEntryOptions options)
        {
            if (absoluteExpiration.HasValue && options.SlidingExpiration.HasValue)
            {
                return (long)Math.Min(
                    (absoluteExpiration.Value - creationTime).TotalSeconds,
                    options.SlidingExpiration.Value.TotalSeconds);
            }
            else if (absoluteExpiration.HasValue)
            {
                return (long)(absoluteExpiration.Value - creationTime).TotalSeconds;
            }
            else if (options.SlidingExpiration.HasValue)
            {
                return (long)options.SlidingExpiration.Value.TotalSeconds;
            }
            return null;
        }

        private static DateTimeOffset? GetAbsoluteExpiration(DateTimeOffset creationTime, DistributedCacheEntryOptions options)
        {
            if (options.AbsoluteExpiration.HasValue && options.AbsoluteExpiration <= creationTime)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(DistributedCacheEntryOptions.AbsoluteExpiration),
                    options.AbsoluteExpiration.Value,
                    "The absolute expiration value must be in the future.");
            }
            var absoluteExpiration = options.AbsoluteExpiration;
            if (options.AbsoluteExpirationRelativeToNow.HasValue)
            {
                absoluteExpiration = creationTime + options.AbsoluteExpirationRelativeToNow;
            }

            return absoluteExpiration;
        }

        public string GetKeyForRedis(string key)
        {
            return $"{_instance}{key}";
        }

        public async Task ExecuteAsync(string command, params object[] args)
        {
            if (command == null)
            {
                throw new ArgumentNullException(nameof(command));
            }

            await ConnectAsync();

            await _cache.ExecuteAsync(command, args);
        }

        public async Task ExecuteAsync(string command, ICollection<object> args, CommandFlags flags = CommandFlags.None)
        {
            if (command == null)
            {
                throw new ArgumentNullException(nameof(command));
            }
            await ConnectAsync();

            await _cache.ExecuteAsync(command, args, flags);
        }

        public async Task<long> HashDecrementAsync(string key, RedisValue hashField, long value = 1, CommandFlags flags = CommandFlags.None, CancellationToken token = default)
        {
            if (key == null)
            {
                throw new ArgumentNullException(nameof(key));
            }
            token.ThrowIfCancellationRequested();
            await ConnectAsync(token: token);

            return await _cache.HashDecrementAsync(GetKeyForRedis(key), hashField, value, flags);
        }

        public async Task<double> HashDecrementAsync(string key, RedisValue hashField, double value, CommandFlags flags = CommandFlags.None, CancellationToken token = default)
        {
            if (key == null)
            {
                throw new ArgumentNullException(nameof(key));
            }
            token.ThrowIfCancellationRequested();
            await ConnectAsync(token: token);

            return await _cache.HashDecrementAsync(GetKeyForRedis(key), hashField, value, flags);
        }

        public async Task<bool> HashDeleteAsync(string key, RedisValue hashField, CommandFlags flags = CommandFlags.None, CancellationToken token = default)
        {
            if (key == null)
            {
                throw new ArgumentNullException(nameof(key));
            }
            token.ThrowIfCancellationRequested();
            await ConnectAsync(token: token);

            return await _cache.HashDeleteAsync(GetKeyForRedis(key), hashField, flags);
        }

        public async Task<long> HashDeleteAsync(string key, RedisValue[] hashFields, CommandFlags flags = CommandFlags.None, CancellationToken token = default)
        {
            if (key == null)
            {
                throw new ArgumentNullException(nameof(key));
            }
            token.ThrowIfCancellationRequested();
            await ConnectAsync(token: token);

            return await _cache.HashDeleteAsync(GetKeyForRedis(key), hashFields, flags);
        }

        public async Task<bool> HashExistsAsync(string key, RedisValue hashField, CommandFlags flags = CommandFlags.None, CancellationToken token = default)
        {
            if (key == null)
            {
                throw new ArgumentNullException(nameof(key));
            }
            token.ThrowIfCancellationRequested();
            await ConnectAsync(token: token);

            return await _cache.HashExistsAsync(GetKeyForRedis(key), hashField, flags);
        }

        public async Task<HashEntry[]> HashGetAllAsync(string key, CommandFlags flags = CommandFlags.None, CancellationToken token = default)
        {
            if (key == null)
            {
                throw new ArgumentNullException(nameof(key));
            }
            token.ThrowIfCancellationRequested();
            await ConnectAsync(token: token);
            return await _cache.HashGetAllAsync(GetKeyForRedis(key), flags);
        }

        public async Task<RedisValue> HashGetAsync(string key, RedisValue hashField, CommandFlags flags = CommandFlags.None, CancellationToken token = default)
        {
            if (key == null)
            {
                throw new ArgumentNullException(nameof(key));
            }
            token.ThrowIfCancellationRequested();
            await ConnectAsync(token: token);
            return await _cache.HashGetAsync(GetKeyForRedis(key), hashField, flags);
        }

        public async Task<RedisValue[]> HashGetAsync(string key, RedisValue[] hashFields, CommandFlags flags = CommandFlags.None, CancellationToken token = default)
        {
            if (key == null)
            {
                throw new ArgumentNullException(nameof(key));
            }
            token.ThrowIfCancellationRequested();

            return await _cache.HashGetAsync(GetKeyForRedis(key), hashFields, flags);
        }

        public async Task<long> HashIncrementAsync(string key, RedisValue hashField, long value = 1, CommandFlags flags = CommandFlags.None, CancellationToken token = default)
        {
            if (key == null)
            {
                throw new ArgumentNullException(nameof(key));
            }
            token.ThrowIfCancellationRequested();

            await ConnectAsync(token: token);
            return await _cache.HashIncrementAsync(GetKeyForRedis(key), hashField, value, flags);
        }

        public async Task<double> HashIncrementAsync(string key, RedisValue hashField, double value, CommandFlags flags = CommandFlags.None, CancellationToken token = default)
        {
            if (key == null)
            {
                throw new ArgumentNullException(nameof(key));
            }
            token.ThrowIfCancellationRequested();

            await ConnectAsync(token: token);
            return await _cache.HashIncrementAsync(GetKeyForRedis(key), hashField, value, flags);
        }

        public async Task<RedisValue[]> HashKeysAsync(string key, CommandFlags flags = CommandFlags.None, CancellationToken token = default)
        {
            if (key == null)
            {
                throw new ArgumentNullException(nameof(key));
            }
            token.ThrowIfCancellationRequested();

            await ConnectAsync(token: token);

            return await _cache.HashKeysAsync(GetKeyForRedis(key), flags);
        }

        public async Task<long> HashLengthAsync(string key, CommandFlags flags = CommandFlags.None, CancellationToken token = default)
        {
            if (key == null)
            {
                throw new ArgumentNullException(nameof(key));
            }
            token.ThrowIfCancellationRequested();

            await ConnectAsync(token: token);
            return await _cache.HashLengthAsync(GetKeyForRedis(key), flags);
        }

        public async Task HashSetAsync(string key, HashEntry[] hashFields, CommandFlags flags = CommandFlags.None, CancellationToken token = default)
        {

            if (key == null)
            {
                throw new ArgumentNullException(nameof(key));
            }
            token.ThrowIfCancellationRequested();

            await ConnectAsync(token: token);
            await _cache.HashSetAsync(GetKeyForRedis(key), hashFields, flags);
        }

        public async Task<bool> HashSetAsync(string key, RedisValue hashField, RedisValue value, When when = When.Always, CommandFlags flags = CommandFlags.None, CancellationToken token = default)
        {
            if (key == null)
            {
                throw new ArgumentNullException(nameof(key));
            }
            token.ThrowIfCancellationRequested();

            await ConnectAsync(token: token);

            return await _cache.HashSetAsync(GetKeyForRedis(key), hashField, value, when, flags);
        }

        public async Task<RedisValue[]> HashValuesAsync(string key, CommandFlags flags = CommandFlags.None, CancellationToken token = default)
        {
            if (key == null)
            {
                throw new ArgumentNullException(nameof(key));
            }
            token.ThrowIfCancellationRequested();

            await ConnectAsync(token: token);

            return await _cache.HashValuesAsync(GetKeyForRedis(key), flags);
        }

        public async Task<bool> KeyDeleteAsync(string key, CommandFlags flags = CommandFlags.None, CancellationToken token = default)
        {
            if (key == null)
            {
                throw new ArgumentNullException(nameof(key));
            }

            token.ThrowIfCancellationRequested();

            await ConnectAsync(token: token);

            return await _cache.KeyDeleteAsync(GetKeyForRedis(key), flags);
        }

        public async Task<long> KeyDeleteAsync(string[] keys, CommandFlags flags = CommandFlags.None, CancellationToken token = default)
        {
            var array = (from string key in keys select (RedisKey)GetKeyForRedis(key)).ToArray();
            await ConnectAsync(token: token);

            return await _cache.KeyDeleteAsync(array, flags);
        }

        public async Task<bool> KeyExistsAsync(string key, CommandFlags flags = CommandFlags.None, CancellationToken token = default)
        {
            if (key == null)
            {
                throw new ArgumentNullException(nameof(key));
            }

            token.ThrowIfCancellationRequested();

            await ConnectAsync(token: token);

            return await _cache.KeyExistsAsync(GetKeyForRedis(key), flags);
        }

        public async Task<long> KeyExistsAsync(string[] keys, CommandFlags flags = CommandFlags.None, CancellationToken token = default)
        {
            var array = (from string key in keys select (RedisKey)GetKeyForRedis(key)).ToArray();
            token.ThrowIfCancellationRequested();

            await ConnectAsync(token: token);

            return await _cache.KeyExistsAsync(array, flags);
        }

        public async Task<bool> KeyExpireAsync(string key, TimeSpan? expiry, CommandFlags flags = CommandFlags.None, CancellationToken token = default)
        {
            if (key == null)
            {
                throw new ArgumentNullException(nameof(key));
            }
            if (expiry == null)
            {
                throw new ArgumentNullException(nameof(expiry));
            }
            token.ThrowIfCancellationRequested();

            await ConnectAsync(token: token);

            return await _cache.KeyExpireAsync(GetKeyForRedis(key), expiry, flags);
        }

        public async Task<bool> KeyExpireAsync(string key, DateTime? expiry, CommandFlags flags = CommandFlags.None, CancellationToken token = default)
        {
            if (key == null)
            {
                throw new ArgumentNullException(nameof(key));
            }
            if (expiry == null)
            {
                throw new ArgumentNullException(nameof(expiry));
            }
            token.ThrowIfCancellationRequested();

            await ConnectAsync(token: token);

            return await _cache.KeyExpireAsync(GetKeyForRedis(key), expiry, flags);
        }

        public async Task<bool> KeyMoveAsync(string key, int database, CommandFlags flags = CommandFlags.None, CancellationToken token = default)
        {
            if (key == null)
            {
                throw new ArgumentNullException(nameof(key));
            }
            token.ThrowIfCancellationRequested();

            await ConnectAsync(token: token);

            return await _cache.KeyMoveAsync(GetKeyForRedis(key), database, flags);
        }

        public async Task<bool> KeyPersistAsync(string key, CommandFlags flags = CommandFlags.None, CancellationToken token = default)
        {
            if (key == null)
            {
                throw new ArgumentNullException(nameof(key));
            }
            token.ThrowIfCancellationRequested();

            await ConnectAsync(token: token);

            return await _cache.KeyPersistAsync(GetKeyForRedis(key), flags);
        }

        public async Task<string> KeyRandomAsync(CommandFlags flags = CommandFlags.None, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();

            await ConnectAsync(token: token);

            return await _cache.KeyRandomAsync(flags);
        }

        public async Task<bool> KeyRenameAsync(string key, string newKey, When when = When.Always, CommandFlags flags = CommandFlags.None, CancellationToken token = default)
        {
            if (key == null)
            {
                throw new ArgumentNullException(nameof(key));
            }
            if (newKey == null)
            {
                throw new ArgumentNullException(nameof(newKey));
            }
            token.ThrowIfCancellationRequested();

            await ConnectAsync(token: token);

            return await _cache.KeyRenameAsync(GetKeyForRedis(key), newKey, when, flags);
        }

        public async Task KeyRestoreAsync(string key, byte[] value, TimeSpan? expiry = null, CommandFlags flags = CommandFlags.None, CancellationToken token = default)
        {
            if (key == null)
            {
                throw new ArgumentNullException(nameof(key));
            }
            token.ThrowIfCancellationRequested();
            await ConnectAsync(token: token);

            await _cache.KeyRestoreAsync(GetKeyForRedis(key), value, expiry, flags);
        }

        public async Task<TimeSpan?> KeyTimeToLiveAsync(string key, CommandFlags flags = CommandFlags.None, CancellationToken token = default)
        {
            if (key == null)
            {
                throw new ArgumentNullException(nameof(key));
            }
            token.ThrowIfCancellationRequested();
            await ConnectAsync(token: token);

            return await _cache.KeyTimeToLiveAsync(GetKeyForRedis(key), flags);
        }

        public async Task<bool> LockExtendAsync(string key, RedisValue value, TimeSpan expiry, CommandFlags flags = CommandFlags.None, CancellationToken token = default)
        {
            if (key == null)
            {
                throw new ArgumentNullException(nameof(key));
            }

            token.ThrowIfCancellationRequested();
            await ConnectAsync(token: token);

            return await _cache.LockExtendAsync(GetKeyForRedis(key), value, expiry, flags);
        }

        public async Task<RedisValue> LockQueryAsync(string key, CommandFlags flags = CommandFlags.None, CancellationToken token = default)
        {
            if (key == null)
            {
                throw new ArgumentNullException(nameof(key));
            }
            token.ThrowIfCancellationRequested();
            await ConnectAsync(token: token);

            return await _cache.LockQueryAsync(GetKeyForRedis(key), flags);
        }

        public async Task<bool> LockReleaseAsync(string key, RedisValue value, CommandFlags flags = CommandFlags.None, CancellationToken token = default)
        {
            if (key == null)
            {
                throw new ArgumentNullException(nameof(key));
            }

            token.ThrowIfCancellationRequested();
            await ConnectAsync(token: token);

            return await _cache.LockReleaseAsync(GetKeyForRedis(key), value, flags);
        }

        public async Task<bool> LockTakeAsync(string key, RedisValue value, TimeSpan expiry, CommandFlags flags = CommandFlags.None, CancellationToken token = default)
        {
            if (key == null)
            {
                throw new ArgumentNullException(nameof(key));
            }

            token.ThrowIfCancellationRequested();
            await ConnectAsync(token: token);

            return await _cache.LockTakeAsync(GetKeyForRedis(key), value, expiry, flags);
        }

        public async Task<bool> SetAddAsync(string key, RedisValue value, CommandFlags flags = CommandFlags.None, CancellationToken token = default)
        {
            if (key == null)
            {
                throw new ArgumentNullException(nameof(key));
            }

            token.ThrowIfCancellationRequested();
            await ConnectAsync(token: token);

            return await _cache.SetAddAsync(GetKeyForRedis(key), value, flags);
        }

        public async Task<bool> SetContainsAsync(string key, RedisValue value, CommandFlags flags = CommandFlags.None, CancellationToken token = default)
        {
            if (key == null)
            {
                throw new ArgumentNullException(nameof(key));
            }

            token.ThrowIfCancellationRequested();
            await ConnectAsync(token: token);

            return await _cache.SetContainsAsync(GetKeyForRedis(key), value, flags);
        }

        public async Task<long> SetLengthAsync(string key, CommandFlags flags = CommandFlags.None, CancellationToken token = default)
        {
            if (key == null)
            {
                throw new ArgumentNullException(nameof(key));
            }

            token.ThrowIfCancellationRequested();
            await ConnectAsync(token: token);

            return await _cache.SetLengthAsync(GetKeyForRedis(key), flags);
        }

        public async Task<RedisValue[]> SetMembersAsync(string key, CommandFlags flags = CommandFlags.None, CancellationToken token = default)
        {
            if (key == null)
            {
                throw new ArgumentNullException(nameof(key));
            }

            token.ThrowIfCancellationRequested();
            await ConnectAsync(token: token);
            return await _cache.SetMembersAsync(GetKeyForRedis(key), flags);
        }

        public async Task<bool> SetMoveAsync(string source, string destination, RedisValue value, CommandFlags flags = CommandFlags.None, CancellationToken token = default)
        {
            if (source == null)
            {
                throw new ArgumentNullException(nameof(source));
            }
            if (destination == null)
            {
                throw new ArgumentNullException(nameof(destination));
            }

            token.ThrowIfCancellationRequested();
            await ConnectAsync(token: token);

            return await _cache.SetMoveAsync(source, destination, value, flags);
        }

        public async Task<RedisValue> SetPopAsync(string key, CommandFlags flags = CommandFlags.None, CancellationToken token = default)
        {
            if (key == null)
            {
                throw new ArgumentNullException(nameof(key));
            }

            token.ThrowIfCancellationRequested();
            await ConnectAsync(token: token);

            return await _cache.SetPopAsync(GetKeyForRedis(key), flags);
        }

        public async Task<RedisValue[]> SetPopAsync(string key, long count, CommandFlags flags = CommandFlags.None, CancellationToken token = default)
        {
            if (key == null)
            {
                throw new ArgumentNullException(nameof(key));
            }

            token.ThrowIfCancellationRequested();
            await ConnectAsync(token: token);

            return await _cache.SetPopAsync(GetKeyForRedis(key), count, flags);
        }

        public async Task<RedisValue> SetRandomMemberAsync(string key, CommandFlags flags = CommandFlags.None, CancellationToken token = default)
        {
            if (key == null)
            {
                throw new ArgumentNullException(nameof(key));
            }

            token.ThrowIfCancellationRequested();
            await ConnectAsync(token: token);

            return await _cache.SetRandomMemberAsync(GetKeyForRedis(key), flags);
        }

        public async Task<RedisValue[]> SetRandomMembersAsync(string key, long count, CommandFlags flags = CommandFlags.None, CancellationToken token = default)
        {
            if (key == null)
            {
                throw new ArgumentNullException(nameof(key));
            }

            token.ThrowIfCancellationRequested();
            await ConnectAsync(token: token);

            return await _cache.SetRandomMembersAsync(GetKeyForRedis(key), count, flags);
        }

        public async Task<bool> SetRemoveAsync(string key, RedisValue value, CommandFlags flags = CommandFlags.None, CancellationToken token = default)
        {
            if (key == null)
            {
                throw new ArgumentNullException(nameof(key));
            }
            token.ThrowIfCancellationRequested();
            await ConnectAsync(token: token);
            return await _cache.SetRemoveAsync(GetKeyForRedis(key), value, flags);
        }

        public async Task<long> SetRemoveAsync(string key, RedisValue[] values, CommandFlags flags = CommandFlags.None, CancellationToken token = default)
        {
            if (key == null)
            {
                throw new ArgumentNullException(nameof(key));
            }
            token.ThrowIfCancellationRequested();
            await ConnectAsync(token: token);

            return await _cache.SetRemoveAsync(GetKeyForRedis(key), values, flags);
        }

        public async Task<bool> SortedSetAddAsync(string key, RedisValue member, double score, CommandFlags flags, CancellationToken token = default)
        {
            if (key == null)
            {
                throw new ArgumentNullException(nameof(key));
            }
            token.ThrowIfCancellationRequested();
            await ConnectAsync(token: token);

            return await _cache.SortedSetAddAsync(GetKeyForRedis(key), member, score, flags);
        }

        public async Task<bool> SortedSetAddAsync(string key, RedisValue member, double score, When when = When.Always, CommandFlags flags = CommandFlags.None, CancellationToken token = default)
        {
            if (key == null)
            {
                throw new ArgumentNullException(nameof(key));
            }
            token.ThrowIfCancellationRequested();
            await ConnectAsync(token: token);

            return await _cache.SortedSetAddAsync(GetKeyForRedis(key), member, score, when, flags);
        }

        public async Task<SortedSetEntry[]> SortedSetRangeByScoreWithScoresAsync(string key, double min, double max, long skip,
            long take, Order order, CancellationToken token = default)
        {
            if (key == null)
            {
                throw new ArgumentNullException(nameof(key));
            }
            token.ThrowIfCancellationRequested();
            await ConnectAsync(token: token);

            return await _cache.SortedSetRangeByScoreWithScoresAsync(GetKeyForRedis(key), min, max, Exclude.None, order, skip, take);
        }

        public async Task<long> StringAppendAsync(string key, string value, CommandFlags flags = CommandFlags.None, CancellationToken token = default)
        {
            if (key == null)
            {
                throw new ArgumentNullException(nameof(key));
            }
            token.ThrowIfCancellationRequested();
            await ConnectAsync(token: token);

            return await _cache.StringAppendAsync(GetKeyForRedis(key), value, flags);
        }

        public async Task<long> DecrementAsync(string key, long value = 1, CommandFlags flags = CommandFlags.None, CancellationToken token = default)
        {
            if (key == null)
            {
                throw new ArgumentNullException(nameof(key));
            }
            token.ThrowIfCancellationRequested();
            await ConnectAsync(token: token);

            return await _cache.StringDecrementAsync(GetKeyForRedis(key), value, flags);
        }

        public async Task<double> DecrementAsync(string key, double value, CommandFlags flags = CommandFlags.None, CancellationToken token = default)
        {
            if (key == null)
            {
                throw new ArgumentNullException(nameof(key));
            }
            token.ThrowIfCancellationRequested();
            await ConnectAsync(token: token);

            return await _cache.StringDecrementAsync(GetKeyForRedis(key), value, flags);
        }

        public async Task<RedisValue> StringGetAsync(string key, CommandFlags flags = CommandFlags.None, CancellationToken token = default)
        {
            if (key == null)
            {
                throw new ArgumentNullException(nameof(key));
            }
            token.ThrowIfCancellationRequested();
            await ConnectAsync(token: token);

            return await _cache.StringGetAsync(GetKeyForRedis(key), flags);
        }


        public async Task<RedisValue[]> StringGetAsync(List<string> keys, CommandFlags flags = CommandFlags.None, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            await ConnectAsync(token: token);

            return await _cache.StringGetAsync(keys.ToRedisKeyArray(_instance), flags);
        }

        public async Task<RedisValue> StringGetRangeAsync(string key, long start, long end, CommandFlags flags = CommandFlags.None, CancellationToken token = default)
        {
            if (key == null)
            {
                throw new ArgumentNullException(nameof(key));
            }
            token.ThrowIfCancellationRequested();
            await ConnectAsync(token: token);

            return await _cache.StringGetRangeAsync(GetKeyForRedis(key), start, end, flags);
        }

        public async Task<RedisValue> StringGetSetAsync(string key, RedisValue value, CommandFlags flags = CommandFlags.None, CancellationToken token = default)
        {
            if (key == null)
            {
                throw new ArgumentNullException(nameof(key));
            }
            token.ThrowIfCancellationRequested();
            await ConnectAsync(token: token);

            return await _cache.StringGetSetAsync(GetKeyForRedis(key), value, flags);
        }

        public async Task<RedisValueWithExpiry> StringGetWithExpiryAsync(string key, CommandFlags flags = CommandFlags.None, CancellationToken token = default)
        {
            if (key == null)
            {
                throw new ArgumentNullException(nameof(key));
            }

            token.ThrowIfCancellationRequested();
            await ConnectAsync(token: token);

            return await _cache.StringGetWithExpiryAsync(GetKeyForRedis(key), flags);
        }

        public async Task<long> IncrementAsync(string key, long value = 1, CommandFlags flags = CommandFlags.None, CancellationToken token = default)
        {
            if (key == null)
            {
                throw new ArgumentNullException(nameof(key));
            }

            token.ThrowIfCancellationRequested();
            await ConnectAsync(token: token);

            return await _cache.StringIncrementAsync(GetKeyForRedis(key), value, flags);
        }

        public async Task<double> IncrementAsync(string key, double value, CommandFlags flags = CommandFlags.None, CancellationToken token = default)
        {
            if (key == null)
            {
                throw new ArgumentNullException(nameof(key));
            }

            token.ThrowIfCancellationRequested();
            await ConnectAsync(token: token);

            return await _cache.StringIncrementAsync(GetKeyForRedis(key), value, flags);
        }

        public async Task<long> StringLengthAsync(string key, CommandFlags flags = CommandFlags.None, CancellationToken token = default)
        {
            if (key == null)
            {
                throw new ArgumentNullException(nameof(key));
            }

            token.ThrowIfCancellationRequested();
            await ConnectAsync(token: token);

            return await _cache.StringLengthAsync(GetKeyForRedis(key), flags);
        }

        public async Task<bool> StringSetAsync(string key, RedisValue value, TimeSpan? expiry = null, When when = When.Always, CommandFlags flags = CommandFlags.None, CancellationToken token = default)
        {
            if (key == null)
            {
                throw new ArgumentNullException(nameof(key));
            }

            token.ThrowIfCancellationRequested();

            await ConnectAsync(token: token);

            return await _cache.StringSetAsync(GetKeyForRedis(key), value, expiry, when, flags);
        }

        public async Task<bool> StringSetBitAsync(string key, long offset, bool bit, CommandFlags flags = CommandFlags.None, CancellationToken token = default)
        {
            if (key == null)
            {
                throw new ArgumentNullException(nameof(key));
            }

            token.ThrowIfCancellationRequested();

            await ConnectAsync(token: token);

            return await _cache.StringSetBitAsync(GetKeyForRedis(key), offset, bit, flags);
        }

        public async Task<string> StringSetRangeAsync(string key, long offset, string value, CommandFlags flags = CommandFlags.None, CancellationToken token = default)
        {
            if (key == null)
            {
                throw new ArgumentNullException(nameof(key));
            }

            token.ThrowIfCancellationRequested();

            await ConnectAsync(token: token);

            return await _cache.StringSetRangeAsync(GetKeyForRedis(key), offset, value, flags);
        }

        public async Task SubscribeAsync<T>(string channel, Action<string, T> handler = null, CommandFlags flags = CommandFlags.None, CancellationToken token = default)
        {
            if (channel == null)
            {
                throw new ArgumentNullException(nameof(channel));
            }
            token.ThrowIfCancellationRequested();
            await ConnectAsync(token: token);
            var sub = _connection.GetSubscriber();
            await sub.SubscribeAsync(channel, (arg1, arg2) =>
           {
               if (handler != null)
               {
                   var value = arg2.DeserializeObject<T>();
                   handler.Invoke(arg1, value);
               }
           }, flags);
        }

        public async Task<long> PublishAsync<T>(string channel, T message, CommandFlags flags = CommandFlags.None, CancellationToken token = default)
        {
            if (channel == null)
            {
                throw new ArgumentNullException(nameof(channel));
            }
            token.ThrowIfCancellationRequested();
            await ConnectAsync(token: token);

            var sub = _connection.GetSubscriber();
            return await sub.PublishAsync(channel, JsonConvert.SerializeObject(message), flags);
        }

        public async Task UnsubscribeAsync<T>(string channel, Action<string, T> handler = null, CommandFlags flags = CommandFlags.None, CancellationToken token = default)
        {
            if (channel == null)
            {
                throw new ArgumentNullException(nameof(channel));
            }
            token.ThrowIfCancellationRequested();
            await ConnectAsync(token: token);
            var sub = _connection.GetSubscriber();
            await sub.UnsubscribeAsync(channel, (arg1, arg2) =>
            {
                if (handler != null)
                {
                    var value = arg2.DeserializeObject<T>();
                    handler.Invoke(arg1, value);
                }
            }, flags);
        }

        public async Task UnsubscribeAll(CommandFlags flags = CommandFlags.None, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            await ConnectAsync(token: token);
            ISubscriber sub = _connection.GetSubscriber();
            await sub.UnsubscribeAllAsync(flags);
        }

        public async Task<T> ObjectGetAsync<T>(T t, CancellationToken token = default)
        {
            if (t == null)
                throw new ArgumentNullException(nameof(t));

            token.ThrowIfCancellationRequested();
            await ConnectAsync(token: token);

            var properties = typeof(T).GetMappingProperties();

            if (properties == null || !properties.Any())
                throw new KeyNotFoundException(nameof(t));

            var mainProp = properties.FirstOrDefault(c => c.MainKey);

            if (mainProp == null)
                throw new KeyNotFoundException($"{nameof(T)}，the main key attribute is not found");

            foreach (var property in properties)
            {
                var objValue = property.Get(t);
             
                if (objValue is null) continue;

                var key = $"{property.FieldName}:{objValue}";

                if (property.MainKey)
                {
                    return await GetAsync<T>(key, token: token);
                }

                var value = await _cache.StringGetAsync(GetKeyForRedis(key));
                if (!value.HasValue)
                    return default;

                return await GetAsync<T>(value, token: token);
            }

            return default;
        }

        public async Task ObjectAddAsync<T>(T t, TimeSpan? expiry = null, CancellationToken token = default)
        {
            if (t == null)
                throw new ArgumentNullException(nameof(t));

            token.ThrowIfCancellationRequested();
            await ConnectAsync(token: token);

            var properties = typeof(T).GetMappingProperties();

            if (properties == null || !properties.Any())
                throw new KeyNotFoundException(nameof(t));

            var keyPairs = new List<KeyValuePair<RedisKey, RedisValue>>();

            var mainProp = properties.FirstOrDefault(c => c.MainKey);

            if (mainProp == null)
                throw new KeyNotFoundException($"{nameof(T)}，the main key is not found");

            var mainKey = $"{mainProp.FieldName}:{mainProp.Get(t)}";

            if (string.IsNullOrEmpty(mainKey))
                throw new ArgumentNullException($"{nameof(T)}，the main key is not null");

            keyPairs.Add(new KeyValuePair<RedisKey, RedisValue>(GetKeyForRedis(mainKey), JsonConvert.SerializeObject(t)));

            foreach (var property in properties.Where(c => !c.MainKey))
            {
                var key = $"{property.FieldName}:{property.Get(t)}";
                keyPairs.Add(new KeyValuePair<RedisKey, RedisValue>(GetKeyForRedis(key),mainKey));
            }

            var transaction = _cache.CreateTransaction();
            foreach (var keyPair in keyPairs)
            {
                transaction.StringSetAsync(keyPair.Key, keyPair.Value, expiry);
            }
            await transaction.ExecuteAsync();
        }

        public async Task ObjectKeyDeleteAsync<T>(T t, CancellationToken token = default)
        {
            if (t == null)
                throw new ArgumentNullException(nameof(t));

            token.ThrowIfCancellationRequested();
            await ConnectAsync(token: token);

            var properties = typeof(T).GetMappingProperties();

            if (properties == null || !properties.Any())
                throw new KeyNotFoundException(nameof(t));

            var keys = new List<string>();
            foreach (var property in properties)
            {
                var key = $"{property.FieldName}:{property.Get(t)}";
                keys.Add(key);
            }
            var array = (from string key in keys select (RedisKey)GetKeyForRedis(key)).ToArray();
            await _cache.KeyDeleteAsync(array);
        }

        public async Task<IRedLock> CreateLockAsync(string resource, TimeSpan expiryTime, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            await ConnectAsync(token: token);
            return await _redLockFactory.CreateLockAsync(resource, expiryTime);
        }

        public async Task<IRedLock> CreateLockAsync(string resource, TimeSpan expiryTime, TimeSpan waitTime, TimeSpan retryTime, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            await ConnectAsync(token: token);
            return await _redLockFactory.CreateLockAsync(resource, expiryTime, waitTime, retryTime);
        }

        public async Task<T> GetAsync<T>(string key, CommandFlags flags = CommandFlags.None, CancellationToken token = default)
        {
            return (await StringGetAsync(key, flags, token)).DeserializeObject<T>();
        }

        public Task<bool> AddAsync<T>(string key, T value, TimeSpan? expiry = null, When when = When.Always, CommandFlags flags = CommandFlags.None, CancellationToken token = default)
        {
            return StringSetAsync(key, JsonConvert.SerializeObject(value), expiry, when, flags, token);
        }


        public void Connect()
        {
            var db = _options.ConfigurationOptions.DefaultDatabase ?? 0;

            if (db < 0) throw new ArgumentOutOfRangeException(nameof(db));

            if (_cache != null)
            {
                return;
            }

            _connectionLock.Wait();
            try
            {
                if (_cache != null) return;
                _connection = _options.ConfigurationOptions != null ? ConnectionMultiplexer.Connect(_options.ConfigurationOptions) : ConnectionMultiplexer.Connect(_options.Configuration);
                _cache = _connection.GetDatabase(db);

                var multiplexers = new List<RedLockMultiplexer>
                {
                    _connection
                };
                _redLockFactory = RedLockFactory.Create(multiplexers);
            }
            finally
            {
                _connectionLock.Release();
            }
        }

        public async Task ConnectAsync(CancellationToken token = default)
        {
            var db = _options.ConfigurationOptions.DefaultDatabase ?? 0;

            if (db < 0) throw new ArgumentOutOfRangeException(nameof(db));

            token.ThrowIfCancellationRequested();

            if (_cache != null)
            {
                return;
            }

            await _connectionLock.WaitAsync(token);
            try
            {
                if (_cache == null)
                {
                    if (_options.ConfigurationOptions != null)
                    {
                        _connection = await ConnectionMultiplexer.ConnectAsync(_options.ConfigurationOptions);
                    }
                    else
                    {
                        _connection = await ConnectionMultiplexer.ConnectAsync(_options.Configuration);
                    }

                    _cache = _connection.GetDatabase(db);

                    var multiplexers = new List<RedLockMultiplexer>
                    {
                        _connection
                    };
                    _redLockFactory = RedLockFactory.Create(multiplexers);
                }
            }
            finally
            {
                _connectionLock.Release();
            }
        }

        public ConnectionMultiplexer GetConnection()
        {
            Connect();
            return _connection;
        }

        public async Task<ConnectionMultiplexer> GetConnectionAsync(CancellationToken token = default)
        {
            await ConnectAsync(token: token);
            return _connection;
        }

        public IDatabase GetDatabase()
        {
            Connect();
            return _cache;
        }

        public async Task<IDatabase> GetDatabaseAsync(CancellationToken token = default)
        {
            await ConnectAsync(token);
            return _cache;
        }

        public void Dispose()
        {
            _connection?.Close();
        }

    }
}