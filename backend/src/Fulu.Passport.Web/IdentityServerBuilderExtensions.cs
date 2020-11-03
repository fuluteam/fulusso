using System;
using System.Text;
using Fulu.Passport.Domain.Options;
using Fulu.Passport.Web.Endpoints;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class IdentityServerBuilderExtensions
    {
        public static IIdentityServerBuilder AddDingTalk(
            this IIdentityServerBuilder builder, Action<ExternalDingTalkOptions> configuration)
        {
            builder.Services.Configure(configuration);
            builder.AddEndpoint<DingTalkEndpoint>("ding", "/ding");
            return builder;
        }
        public static IIdentityServerBuilder AddWechat(
            this IIdentityServerBuilder builder, Action<ExternalWeChatOptions> configuration)
        {
            builder.Services.Configure(configuration);
            builder.AddEndpoint<WeChatEndpoint>("wechat", "/wechat");
            return builder;
        }
    }
}