using System;
using System.Collections.Generic;
using System.Text;

namespace Microsoft.Extensions.Caching.Redis
{
    public class RedisKeyAttribute : Attribute
    {
        public string PrefixName { get; set; }
        public bool PrimaryKey { get; set; } = false;
    }
}
