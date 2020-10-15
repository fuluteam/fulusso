using System;
using System.Collections.Generic;
using System.Text;

namespace Microsoft.Extensions.Caching.Redis
{
    public class RedisKeyAttribute : Attribute
    {
        public bool MainKey { get; set; }

        public string FieldName { get; set; }

        public RedisKeyAttribute()
        {
            MainKey = true;
        }
        public RedisKeyAttribute(string fieldName, bool mainKey = false)
        {
            FieldName = fieldName;
            MainKey = mainKey;
        }
    }
}
