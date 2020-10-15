using System;
using System.Collections.Concurrent;
using System.Collections.Generic;

namespace Fulu.Core.PropertyWrapper
{
    /// <summary>
    /// 缓存服务
    /// </summary>
    public class CacheService
    {
        public static void Add<T>(string key, T value, int expireSec = 0)
        {
            CacheBody<T>.GetInstance().Add(key, value, expireSec);
        }

        public static bool ContainsKey(string key)
        {
            return CacheBody<string>.GetInstance().ContainsKey(key);
        }

        public static bool ContainsKey<T>(string key)
        {
            return CacheBody<T>.GetInstance().ContainsKey(key);
        }

        public static string Get(string key)
        {
            return CacheBody<string>.GetInstance().Get(key);
        }

        public static T Get<T>(string key)
        {
            return CacheBody<T>.GetInstance().Get(key);
        }

        public static IEnumerable<string> GetAllKey<T>()
        {
            return CacheBody<T>.GetInstance().GetAllKey();
        }

        public static T GetOrCreate<T>(string key, Func<T> create, int expireSec)
        {
            return CacheBody<T>.GetInstance().GetOrCreate(key, create, expireSec);
        }

        public static void Remove<T>(string key)
        {
            CacheBody<T>.GetInstance().Remove(key);
        }

        public static void Update<T>(string key, T value, int expireSec = 0)
        {
            CacheBody<T>.GetInstance().Update(key, value, expireSec);
        }
    }

    public class CacheBody<T>
    {
        partial class CacheWithExpire<T2>
        {
            public T2 Value { get; set; }

            /// <summary>
            /// 过期时间
            /// </summary>
            public DateTime? ExpireTime { get; set; }
        }

        readonly ConcurrentDictionary<string, CacheWithExpire<T>> CacheModel =
            new ConcurrentDictionary<string, CacheWithExpire<T>>();

        private static CacheBody<T> _instance = null;
        private static readonly object locker = new object();

        private CacheBody()
        {
        }

        public static CacheBody<T> GetInstance()
        {
            if (_instance == null)
            {
                lock (locker)
                {
                    if (_instance == null)
                    {
                        _instance = new CacheBody<T>();
                    }
                }
            }

            return _instance;
        }

        public bool ContainsKey(string key)
        {
            return CacheModel.ContainsKey(key);
        }

        public T Get(string key)
        {
            if (ContainsKey(key))
            {
                var cacheValue = CacheModel[key];
                if (cacheValue.ExpireTime != null && cacheValue.ExpireTime < DateTime.Now)
                {
                    //已过期，移除过期的项
                    Remove(key);
                    return default(T);
                }

                return cacheValue.Value;
            }

            return default(T);
        }

        public T this[string key] => Get(key);

        public void Add(string key, T value, int expireSec = 0)
        {
            DateTime? expireDate = null;
            if (expireSec > 0)
            {
                expireDate = DateTime.Now.AddSeconds(expireSec);
            }

            var v = new CacheWithExpire<T>
            {
                ExpireTime = expireDate,
                Value = value
            };
            CacheModel.GetOrAdd(key, v);
        }

        public void Update(string key, T value, int expireSec = 0)
        {
            if (!ContainsKey(key))
            {
                Add(key, value);
            }
            else
            {
                var currentValue = new CacheWithExpire<T> { Value = value };
                if (expireSec > 0)
                {
                    currentValue.ExpireTime = DateTime.Now.AddSeconds(expireSec);
                }

                CacheModel.TryUpdate(key, currentValue, null);
            }
        }

        public void RemoveAll()
        {
            foreach (var item in GetAllKey())
            {
                Remove(item);
            }
        }

        public IEnumerable<string> GetAllKey()
        {
            return CacheModel.Keys;
        }

        public void Remove(string key)
        {
            CacheWithExpire<T> val;
            CacheModel.TryRemove(key, out val);
        }

        public T GetOrCreate(string key, Func<T> createFunc, int expireSec = 0)
        {
            if (ContainsKey(key))
                return Get(key);
            var newval = createFunc();
            Add(key, newval, expireSec);
            return newval;
        }
    }
}