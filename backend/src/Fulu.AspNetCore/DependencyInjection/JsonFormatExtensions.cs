using System;
using System.Collections.Generic;
using System.Text;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class JsonFormatExtensions
    {
        public static IMvcBuilder AddJsonDateFormatter(this IMvcBuilder builder)
        {
#if NETCOREAPP2_1
            builder.AddJsonOptions(options =>
            {
                options.SerializerSettings.DateFormatString = "yyyy-MM-dd HH:mm:ss";
            });
#endif
#if NETCOREAPP3_1
            builder.AddNewtonsoftJson(opt => opt.SerializerSettings.DateFormatString = "yyyy-MM-dd HH:mm:ss");
#endif
            return builder;
        }
    }
}
