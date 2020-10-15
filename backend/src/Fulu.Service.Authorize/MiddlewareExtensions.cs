namespace Microsoft.AspNetCore.Builder
{
    public static class MiddlewareExtensions
    {
        public static IApplicationBuilder UseIchAuthorize(this IApplicationBuilder builder)
        {
            builder.UseAuthentication();
            return builder.UseMiddleware<IchAuthorizeMiddleware>();
        }
    }
}