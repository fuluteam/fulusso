using Microsoft.Extensions.Options;
using System;
using System.Threading.Tasks;
using System.Linq;
using Fulu.WebAPI.Abstractions;
using Microsoft.AspNetCore.Authorization;
using IdentityModel;
using Fulu.Authentication;

namespace Microsoft.AspNetCore.Mvc.Filters
{
    public class ServiceAuthorizeFilter : IAsyncResourceFilter
    {
        private readonly IAuthorizeTokenClient _serviceAuthorizeHttpClient;
        private readonly ServiceAuthorizeOptions _options;

        public ServiceAuthorizeFilter(IAuthorizeTokenClient serviceAuthorizeHttpClient, IOptions<ServiceAuthorizeOptions> options)
        {
            _serviceAuthorizeHttpClient = serviceAuthorizeHttpClient;
            _options = options?.Value ?? throw new ArgumentNullException(nameof(options));
        }

        public async Task OnResourceExecutionAsync(ResourceExecutingContext context, ResourceExecutionDelegate next)
        {
            if (!context.HttpContext.User.Identity.IsAuthenticated || !context.Filters.ExistsAuthorizeAttribute())
            {
                await next();
                return;
            }

            var method = context.GetMethod();
            var authorization = context.GetAuthorization();

            var onClientValidate = _options.OnClientValidate;

            var clientValidate = ClientValidate.None; 
            if (context.Filters.OfType<BasicAuthorizeAttribute>().Any())
                clientValidate = context.Filters.OfType<BasicAuthorizeAttribute>().First().ClientValidate;

            if (clientValidate != ClientValidate.None)
            {
                onClientValidate = (clientValidate == ClientValidate.On);
            }

            if (onClientValidate)
            {
                var clientId = context.HttpContext.User.GetClientId();
                if (_options.ClientId != clientId)
                {
                    if (string.IsNullOrWhiteSpace(_options.Authority))
                        throw new ArgumentNullException(nameof(_options.Authority));

                    var grantInfoResponse = await _serviceAuthorizeHttpClient.GetGrantInfo(
                        method, authorization);
                    if (grantInfoResponse.Code != "0")
                    {
                        context.Result = new BadRequestObjectResult(ResponseResult.Ok(grantInfoResponse.Code, grantInfoResponse.Message));
                        return;
                    }

                    var grantInfo = grantInfoResponse.Data;
                    if (grantInfo == null)
                    {
                        context.Result = new BadRequestObjectResult(ResponseResult.Ok("-1", "获取授权信息失败"));
                        return;
                    }

                    if (!grantInfo.Granted)
                    {
                        context.Result = new ObjectResult(ResponseResult.Ok("-1", "该资源需要appid拥有授权")) { StatusCode = 403 };
                        return;
                    }
                }
            }

            await next();
        }
    }
}