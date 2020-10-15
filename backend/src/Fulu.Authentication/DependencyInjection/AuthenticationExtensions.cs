using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Filters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace Microsoft.AspNetCore.Authorization
{
    public static class AuthenticationUtilities
    {
        public static string GetMethod(this ResourceExecutingContext context)
        {
            return GetMethod(context.ActionDescriptor.AttributeRouteInfo.Template, context.HttpContext.Request.Method).ToLower();
        }

        private static string GetMethod(string routeTemplate, string requestMethod)
        {
            var parts = (routeTemplate + "/" + requestMethod)
                .Split('/');
            var builder = new StringBuilder();
            builder.Append($"{Assembly.GetEntryAssembly()?.ManifestModule.Name.Replace(".dll", "")}.");
            foreach (var part in parts)
            {
                var trimmed = part.Trim('{', '}');
                builder.AppendFormat("{0}{1}",
                    part.StartsWith("{") ? "By" : string.Empty,
                    ToTitleCase(trimmed)
                );
            }
            return builder.ToString();
        }

        private static string ToTitleCase(string value)
        {
            if (string.IsNullOrEmpty(value)) return value;
            return char.ToUpperInvariant(value[0]) + value.Substring(1);
        }

        public static bool ExistsAuthorizeAttribute(this IEnumerable<IFilterMetadata> filters)
        {
            return filters.OfType<BasicAuthorizeAttribute>().Any();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        public static string GetAuthorization(this ResourceExecutingContext context)
        {
            return context.HttpContext.Request.Headers["Authorization"];
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        public static string GetAuthorization(this HttpContext context)
        {
            return context.Request.Headers["Authorization"];
        }
    }
}
