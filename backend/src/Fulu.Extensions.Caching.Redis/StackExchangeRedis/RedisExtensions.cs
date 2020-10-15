// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Fulu.Core.PropertyWrapper;
using Newtonsoft.Json;
using StackExchange.Redis;

namespace Microsoft.Extensions.Caching.Redis
{
    public static class RedisExtensions
    {
        private const string HmGetScript = (@"return redis.call('HMGET', KEYS[1], unpack(ARGV))");

        public static RedisValue[] HashMemberGet(this IDatabase cache, string key, params string[] members)
        {
            var result = cache.ScriptEvaluate(
                HmGetScript,
                new RedisKey[] { key },
                GetRedisMembers(members));

            // TODO: Error checking?
            return (RedisValue[])result;
        }

        public static async Task<RedisValue[]> HashMemberGetAsync(
            this IDatabase cache,
            string key,
            params string[] members)
        {
            var result = await cache.ScriptEvaluateAsync(
                HmGetScript,
                new RedisKey[] { key },
                GetRedisMembers(members));

            // TODO: Error checking?
            return (RedisValue[])result;
        }

        public static RedisValue[] GetRedisMembers(params string[] members)
        {
            var redisMembers = new RedisValue[members.Length];
            for (int i = 0; i < members.Length; i++)
            {
                redisMembers[i] = members[i];
            }

            return redisMembers;
        }

        public static List<T> ToList<T>(this RedisValue[] redisValues)
        {
            var values = new List<T>();
            foreach (var redisValue in redisValues)
            {
                var value = default(T);
                if (!redisValue.IsNull)
                    value = JsonConvert.DeserializeObject<T>(redisValue);
                values.Add(value);
            }
            return values;
        }

        public static T DeserializeObject<T>(this RedisValue redisValue)
        {
            var value = default(T);
            if (!redisValue.IsNull)
                value = JsonConvert.DeserializeObject<T>(redisValue);
            return value;
        }

        public static List<KeyValuePair<string, T>> ToKeyValuePairs<T>(this HashEntry[] hashEntries)
        {
            var keyValuePairs = new List<KeyValuePair<string, T>>();
            foreach (var hashEntry in hashEntries)
            {
                var value = default(T);
                if (!hashEntry.Value.IsNull)
                    value = JsonConvert.DeserializeObject<T>(hashEntry.Value);
                keyValuePairs.Add(new KeyValuePair<string, T>(hashEntry.Name, value));
            }
            return keyValuePairs;
        }

        public static HashEntry[] ToHashEntryArray(this List<KeyValuePair<string, RedisValue>> pairs)
        {
            var hashEntries = new HashEntry[pairs.Count];
            for (var i = 0; i < pairs.Count; i++)
            {
                hashEntries[i] = new HashEntry(pairs[i].Key, pairs[i].Value);
            }
            return hashEntries;
        }

        public static RedisValue[] ToRedisValueArray<T>(this List<T> values)
        {
            var redisValues = new RedisValue[values.Count];
            for (int i = 0; i < redisValues.Length; i++)
            {
                redisValues[i] = JsonConvert.SerializeObject(values[i]);
            }
            return redisValues;
        }

        public static RedisKey[] ToRedisKeyArray(this List<string> values, string instanceName)
        {
            var redisValues = new RedisKey[values.Count];
            for (int i = 0; i < redisValues.Length; i++)
            {
                redisValues[i] = $"{instanceName}{values[i]}";
            }
            return redisValues;
        }

        /// <summary>
        /// 获取Unix时间戳（Unix timestamp）
        /// </summary>
        /// <returns></returns>
        public static int ToUnixTimestamp(this DateTime dateTime)
        {
            var unixTimestampZeroPoint = new DateTime(1970, 01, 01, 0, 0, 0, DateTimeKind.Utc);
            return (int)(dateTime - unixTimestampZeroPoint).TotalSeconds;
        }

        public static List<RedisEntityInfo> GetMappingProperties(this System.Type type)
        {
            var key = $"Caching.{type.FullName}";
            var list = CacheService.Get<List<RedisEntityInfo>>(key);
            if (list != null) return list;
            list = new List<RedisEntityInfo>();
            foreach (var propertyInfo in type.GetProperties())
            {
                //if (propertyInfo.PropertyType.Name != "Nullable`1") continue;

                var attr = propertyInfo.GetCustomAttribute<RedisKeyAttribute>();
                if (attr == null)
                {
                    continue;
                }

                var temp = new RedisEntityInfo
                {
                    PropertyInfo = propertyInfo,
                    PropertyName = propertyInfo.Name,
                    GetMethod = propertyInfo.CreateGetter(),
                    MainKey = attr.MainKey,
                    FieldName = attr.FieldName
                };
                list.Add(temp);
            }
            CacheService.Add(key, list);
            return list;
        }
    }
}