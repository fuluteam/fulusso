using System;
using System.Collections.Generic;
using System.Text;

namespace FuLu.Passport.Domain.Options
{
    public class RedisOptions
    {
        public int Database { get; set; }

        public string ConnectionString { get; set; }

        public string InstanceName { get; set; }
    }
}
