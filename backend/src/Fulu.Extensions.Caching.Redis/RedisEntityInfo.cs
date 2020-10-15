using Fulu.Core.PropertyWrapper;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace Microsoft.Extensions.Caching.Redis
{
    public class RedisEntityInfo
    {
        public PropertyInfo PropertyInfo { get; set; }

        public string PropertyName { get; set; }

        public bool MainKey { get; set; }

        public string FieldName { get; set; }

        public IGetValue GetMethod { get; set; }
        public object Get(object obj)
        {
            return this.GetMethod.Get(obj);
        }
    }
}
