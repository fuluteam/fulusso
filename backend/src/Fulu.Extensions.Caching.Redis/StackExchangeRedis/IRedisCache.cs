using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Distributed;
using RedLockNet;
using StackExchange.Redis;

namespace Microsoft.Extensions.Caching.Redis
{
    public interface IRedisCache
    {
        /// <summary>
        /// Gets a value with the given key.
        /// </summary>
        /// <param name="key">A string identifying the requested value.</param>
        /// <returns>The located value or null.</returns>
        byte[] GetAndRefresh(string key);

        /// <summary>
        /// Gets a value with the given key.
        /// </summary>
        /// <param name="key">A string identifying the requested value.</param>
        /// <param name="token">Optional. The <see cref="CancellationToken"/> used to propagate notifications that the operation should be canceled.</param>
        /// <returns>The <see cref="Task"/> that represents the asynchronous operation, containing the located value or null.</returns>
        Task<byte[]> GetAndRefreshAsync(string key, CancellationToken token = default);

        /// <summary>
        /// Sets a value with the given key.
        /// </summary>
        /// <param name="key">A string identifying the requested value.</param>
        /// <param name="value">The value to set in the cache.</param>
        /// <param name="options">The cache options for the value.</param>
        void ScriptEvaluate(string key, byte[] value, DistributedCacheEntryOptions options);

        /// <summary>
        /// Sets the value with the given key.
        /// </summary>
        /// <param name="key">A string identifying the requested value.</param>
        /// <param name="value">The value to set in the cache.</param>
        /// <param name="options">The cache options for the value.</param>
        /// <param name="token">Optional. The <see cref="CancellationToken"/> used to propagate notifications that the operation should be canceled.</param>
        /// <returns>The <see cref="Task"/> that represents the asynchronous operation.</returns>
        Task ScriptEvaluateAsync(string key, byte[] value, DistributedCacheEntryOptions options, CancellationToken token = default);
        string GetKeyForRedis(string key);
        Task ExecuteAsync(string command, params object[] args);
        Task ExecuteAsync(string command, ICollection<object> args, CommandFlags flags = CommandFlags.None);
        Task<long> HashDecrementAsync(string key, RedisValue hashField, long value = 1, CommandFlags flags = CommandFlags.None, CancellationToken token = default);
        Task<double> HashDecrementAsync(string key, RedisValue hashField, double value, CommandFlags flags = CommandFlags.None, CancellationToken token = default);
        Task<bool> HashDeleteAsync(string key, RedisValue hashField, CommandFlags flags = CommandFlags.None, CancellationToken token = default);
        Task<long> HashDeleteAsync(string key, RedisValue[] hashFields, CommandFlags flags = CommandFlags.None, CancellationToken token = default);
        Task<bool> HashExistsAsync(string key, RedisValue hashField, CommandFlags flags = CommandFlags.None, CancellationToken token = default);
        Task<HashEntry[]> HashGetAllAsync(string key, CommandFlags flags = CommandFlags.None, CancellationToken token = default);
        Task<RedisValue> HashGetAsync(string key, RedisValue hashField, CommandFlags flags = CommandFlags.None, CancellationToken token = default);
        Task<RedisValue[]> HashGetAsync(string key, RedisValue[] hashFields, CommandFlags flags = CommandFlags.None, CancellationToken token = default);
        Task<long> HashIncrementAsync(string key, RedisValue hashField, long value = 1, CommandFlags flags = CommandFlags.None, CancellationToken token = default);
        Task<double> HashIncrementAsync(string key, RedisValue hashField, double value, CommandFlags flags = CommandFlags.None, CancellationToken token = default);
        Task<RedisValue[]> HashKeysAsync(string key, CommandFlags flags = CommandFlags.None, CancellationToken token = default);
        Task<long> HashLengthAsync(string key, CommandFlags flags = CommandFlags.None, CancellationToken token = default);
        Task HashSetAsync(string key, HashEntry[] hashFields, CommandFlags flags = CommandFlags.None, CancellationToken token = default);
        Task<bool> HashSetAsync(string key, RedisValue hashField, RedisValue value, When when = When.Always, CommandFlags flags = CommandFlags.None, CancellationToken token = default);
        Task<RedisValue[]> HashValuesAsync(string key, CommandFlags flags = CommandFlags.None, CancellationToken token = default);
        Task<bool> KeyDeleteAsync(string key, CommandFlags flags = CommandFlags.None, CancellationToken token = default);
        Task<long> KeyDeleteAsync(string[] keys, CommandFlags flags = CommandFlags.None, CancellationToken token = default);
        Task<bool> KeyExistsAsync(string key, CommandFlags flags = CommandFlags.None, CancellationToken token = default);
        Task<long> KeyExistsAsync(string[] keys, CommandFlags flags = CommandFlags.None, CancellationToken token = default);
        Task<bool> KeyExpireAsync(string key, TimeSpan? expiry, CommandFlags flags = CommandFlags.None, CancellationToken token = default);
        Task<bool> KeyExpireAsync(string key, DateTime? expiry, CommandFlags flags = CommandFlags.None, CancellationToken token = default);
        Task<bool> KeyMoveAsync(string key, int database, CommandFlags flags = CommandFlags.None, CancellationToken token = default);
        Task<bool> KeyPersistAsync(string key, CommandFlags flags = CommandFlags.None, CancellationToken token = default);
        Task<string> KeyRandomAsync(CommandFlags flags = CommandFlags.None, CancellationToken token = default);
        Task<bool> KeyRenameAsync(string key, string newKey, When when = When.Always, CommandFlags flags = CommandFlags.None, CancellationToken token = default);
        Task KeyRestoreAsync(string key, byte[] value, TimeSpan? expiry = null, CommandFlags flags = CommandFlags.None, CancellationToken token = default);
        Task<TimeSpan?> KeyTimeToLiveAsync(string key, CommandFlags flags = CommandFlags.None, CancellationToken token = default);
        Task<bool> LockExtendAsync(string key, RedisValue value, TimeSpan expiry, CommandFlags flags = CommandFlags.None, CancellationToken token = default);
        Task<RedisValue> LockQueryAsync(string key, CommandFlags flags = CommandFlags.None, CancellationToken token = default);
        Task<bool> LockReleaseAsync(string key, RedisValue value, CommandFlags flags = CommandFlags.None, CancellationToken token = default);
        Task<bool> LockTakeAsync(string key, RedisValue value, TimeSpan expiry, CommandFlags flags = CommandFlags.None, CancellationToken token = default);
        Task<bool> SetAddAsync(string key, RedisValue values, CommandFlags flags = CommandFlags.None, CancellationToken token = default);
        Task<bool> SetContainsAsync(string key, RedisValue value, CommandFlags flags = CommandFlags.None, CancellationToken token = default);
        Task<long> SetLengthAsync(string key, CommandFlags flags = CommandFlags.None, CancellationToken token = default);
        Task<RedisValue[]> SetMembersAsync(string key, CommandFlags flags = CommandFlags.None, CancellationToken token = default);
        Task<bool> SetMoveAsync(string source, string destination, RedisValue value, CommandFlags flags = CommandFlags.None, CancellationToken token = default);
        Task<RedisValue> SetPopAsync(string key, CommandFlags flags = CommandFlags.None, CancellationToken token = default);
        Task<RedisValue[]> SetPopAsync(string key, long count, CommandFlags flags = CommandFlags.None, CancellationToken token = default);
        Task<RedisValue> SetRandomMemberAsync(string key, CommandFlags flags = CommandFlags.None, CancellationToken token = default);
        Task<RedisValue[]> SetRandomMembersAsync(string key, long count, CommandFlags flags = CommandFlags.None, CancellationToken token = default);
        Task<bool> SetRemoveAsync(string key, RedisValue value, CommandFlags flags = CommandFlags.None, CancellationToken token = default);
        Task<long> SetRemoveAsync(string key, RedisValue[] values, CommandFlags flags = CommandFlags.None, CancellationToken token = default);
        Task<bool> SortedSetAddAsync(string key, RedisValue member, double score, CommandFlags flags, CancellationToken token = default);
        Task<bool> SortedSetAddAsync(string key, RedisValue member, double score, When when = When.Always, CommandFlags flags = CommandFlags.None, CancellationToken token = default);
        Task<SortedSetEntry[]> SortedSetRangeByScoreWithScoresAsync(string key, double min, double max,
            long skip,
            long take, Order order, CancellationToken token = default);
        Task<long> StringAppendAsync(string key, string value, CommandFlags flags = CommandFlags.None, CancellationToken token = default);
        Task<long> DecrementAsync(string key, long value = 1, CommandFlags flags = CommandFlags.None, CancellationToken token = default);
        Task<double> DecrementAsync(string key, double value, CommandFlags flags = CommandFlags.None, CancellationToken token = default);
        Task<RedisValue> StringGetAsync(string key, CommandFlags flags = CommandFlags.None, CancellationToken token = default);
        Task<RedisValue[]> StringGetAsync(List<string> keys, CommandFlags flags = CommandFlags.None,
            CancellationToken token = default);
        Task<RedisValue> StringGetRangeAsync(string key, long start, long end, CommandFlags flags = CommandFlags.None, CancellationToken token = default);
        Task<RedisValue> StringGetSetAsync(string key, RedisValue value, CommandFlags flags = CommandFlags.None, CancellationToken token = default);
        Task<RedisValueWithExpiry> StringGetWithExpiryAsync(string key, CommandFlags flags = CommandFlags.None, CancellationToken token = default);
        Task<long> IncrementAsync(string key, long value = 1, CommandFlags flags = CommandFlags.None, CancellationToken token = default);
        Task<double> IncrementAsync(string key, double value, CommandFlags flags = CommandFlags.None, CancellationToken token = default);
        Task<long> StringLengthAsync(string key, CommandFlags flags = CommandFlags.None, CancellationToken token = default);
        Task<bool> StringSetAsync(string key, RedisValue value, TimeSpan? expiry = null, When when = When.Always,
           CommandFlags flags = CommandFlags.None, CancellationToken token = default);
        Task<bool> StringSetBitAsync(string key, long offset, bool bit, CommandFlags flags = CommandFlags.None, CancellationToken token = default);
        Task<string> StringSetRangeAsync(string key, long offset, string value, CommandFlags flags = CommandFlags.None, CancellationToken token = default);
        Task SubscribeAsync<T>(string channel, Action<string, T> handler = null, CommandFlags flags = CommandFlags.None, CancellationToken token = default);
        //
        // 摘要:
        //     Posts a message to the given channel.
        //
        // 参数:
        //   channel:
        //     The channel to publish to.
        //
        //   message:
        //     The message to publish.
        //
        //   flags:
        //     The command flags to use.
        //
        // 返回结果:
        //     the number of clients that received the message.
        //
        // 言论：
        //     https://redis.io/commands/publish
        Task<long> PublishAsync<T>(string channel, T message, CommandFlags flags = CommandFlags.None, CancellationToken token = default);
        Task UnsubscribeAsync<T>(string channel, Action<string, T> handler = null, CommandFlags flags = CommandFlags.None, CancellationToken token = default);
        Task UnsubscribeAll(CommandFlags flags = CommandFlags.None, CancellationToken token = default);
        Task<T> ObjectGetAsync<T>(T t, CancellationToken token = default);
        //Task ObjectAddAsync<T>(T t, CancellationToken token = default);
        Task ObjectAddAsync<T>(T t, TimeSpan? expiry = null, CancellationToken token = default);
        Task ObjectKeyDeleteAsync<T>(T t, CancellationToken token = default);
        Task<IRedLock> CreateLockAsync(string resource, TimeSpan expiryTime, CancellationToken token = default);
        Task<IRedLock> CreateLockAsync(string resource, TimeSpan expiryTime, TimeSpan waitTime, TimeSpan retryTime,
            CancellationToken token = default);
        Task<T> GetAsync<T>(string key, CommandFlags flags = CommandFlags.None, CancellationToken token = default);
        Task<bool> AddAsync<T>(string key, T value, TimeSpan? expiry = null, When when = When.Always,
            CommandFlags flags = CommandFlags.None, CancellationToken token = default);
        ConnectionMultiplexer GetConnection();
        Task<ConnectionMultiplexer> GetConnectionAsync(CancellationToken token = default);
        IDatabase GetDatabase();
        Task<IDatabase> GetDatabaseAsync(CancellationToken token = default);
        void Dispose();
    }
}
