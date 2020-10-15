using System;
using Microsoft.Extensions.DependencyInjection;
using ICH.Document;

namespace Microsoft.AspNetCore.Builder
{
    public static class ApplicationBuilderExtension
    {
        public static IApplicationBuilder UseDocument(this IApplicationBuilder app, DocumentOAuthConfig config = null)
        {
            app.UseSwagger();
            app.UseSwaggerUI(c =>
            {
                c.SwaggerEndpoint("/swagger/v3/swagger.json", "My API V3");
                if (config == null) return;
                c.OAuthClientId(config.ClientId);
                c.OAuthClientSecret(config.ClientSecret);
                c.OAuthRealm(config.Realm);
                c.OAuthAppName(config.AppName);
                c.OAuthScopeSeparator(config.ScopeSeparator);
            });
            return app;
        }
    }
}
