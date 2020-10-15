using System;
using System.Collections.Generic;
using System.Text;

namespace Microsoft.AspNetCore.Http
{
    public static class HttpContextExtensions
    {
        public static string GetIp(this HttpContext context)
        {
            return context.Connection.RemoteIpAddress?.MapToIPv4().ToString();
        }
    }
}
