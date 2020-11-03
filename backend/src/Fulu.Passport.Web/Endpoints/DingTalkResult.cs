using System;
using System.Collections.Specialized;
using System.Net.Http;
using Fulu.Passport.Domain.Options;
using Fulu.Passport.Domain.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace Fulu.Passport.Web.Endpoints
{
    public class DingTalkResult : ExternalResult
    {

        public DingTalkResult(NameValueCollection values, IServiceProvider serviceProvider) : base(values, serviceProvider)
        {
            var httpFactory = serviceProvider.GetService<IHttpClientFactory>();
            ExternalService = new DingTalkService(httpFactory.CreateClient());
            var option = serviceProvider.GetService<IOptions<ExternalDingTalkOptions>>().Value;
            AppId = option.AppId;
            Secret = option.Secret;
            LoginProvider = "ding";
            AuthUrl = $"https://oapi.dingtalk.com/connect/qrconnect?appid={AppId}&response_type=code&scope=snsapi_login&redirect_uri=#redirect_uri&state=identityserver";
        }
    }
}