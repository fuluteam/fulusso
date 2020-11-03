using System;
using System.Collections.Specialized;
using System.Net.Http;
using Fulu.Passport.Domain.Options;
using Fulu.Passport.Domain.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace Fulu.Passport.Web.Endpoints
{
    public class WeChatResult : ExternalResult
    {
        public WeChatResult(NameValueCollection values, IServiceProvider serviceProvider) : base(values, serviceProvider)
        {
            var httpFactory = serviceProvider.GetService<IHttpClientFactory>();
            ExternalService = new WeChatService(httpFactory.CreateClient());
            var option = serviceProvider.GetService<IOptions<ExternalWeChatOptions>>().Value;
            AppId = option.AppId;
            Secret = option.Secret;
            LoginProvider = "wechat";
            AuthUrl = $"https://open.weixin.qq.com/connect/qrconnect?appid={AppId}&redirect_uri=#redirect_uri&response_type=code&scope=snsapi_login&state=STATE#wechat_redirect";
        }
    }
}