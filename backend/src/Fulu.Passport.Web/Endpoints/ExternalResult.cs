using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using Fulu.Core.Extensions;
using Fulu.Passport.Domain.Interface;
using Fulu.Passport.Domain.Interface.Services;
using FuLu.Passport.Domain.Interface.Services;
using FuLu.Passport.Domain.Models;
using IdentityModel;
using IdentityServer4;
using IdentityServer4.Extensions;
using IdentityServer4.Hosting;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Fulu.Passport.Web.Endpoints
{
    public class ExternalResult : IEndpointResult
    {
        private readonly NameValueCollection _values;
        private readonly IExternalUserService _externalUserService;
        private readonly IUserService _userService;
        private readonly ILogger _logger;
        private readonly string _bindUrl;
        public string AuthUrl { get; set; }
        public string AppId { get; set; }
        public string Secret { get; set; }
        public IExternalService ExternalService { get; set; }
        public string LoginProvider { get; set; }
   
        private string loginUrl;
        public ExternalResult(NameValueCollection values, IServiceProvider serviceProvider)
        {
            _values = values;
            _externalUserService = serviceProvider.GetService<IExternalUserService>();
            _bindUrl = serviceProvider.GetService<IConfiguration>()["UserInteraction:BindUrl"];
            loginUrl = serviceProvider.GetService<IConfiguration>()["UserInteraction:LoginUrl"];
            _userService = serviceProvider.GetService<IUserService>();
            _logger = serviceProvider.GetService<ILogger<ExternalResult>>();
        }
        public async Task ExecuteAsync(HttpContext context)
        {
            var code = _values.Get("code");
            var returnUrl = _values.Get("ReturnUrl");
            if (code.IsNullOrEmpty())
            {
                var redirectUri = $"{context.Request.Scheme}://{context.Request.Host.Value}/{LoginProvider}?ReturnUrl={HttpUtility.UrlEncode(returnUrl, Encoding.UTF8)}";
                context.Response.Redirect(AuthUrl.Replace("#redirect_uri", HttpUtility.UrlEncode(redirectUri, Encoding.UTF8)));
            }
            else
            {
                string openId;
                try
                {
                    openId = await ExternalService.GetOpenId(code, AppId, Secret);
                }
                catch (Exception e)
                {
                    _logger.LogError(e.ToString());
                    context.Response.Redirect(loginUrl);
                    return;
                }

                var clientId = context.User.GetClientId().ToInt32();
                var userId = context.User.GetUserId();
                //判断是否是登录状态，如果是登录状态，则进行绑定
                var amr = context.User.FindFirstValue(JwtClaimTypes.AuthenticationMethod);
                if (context.User.IsAuthenticated() && amr.Contains("mfa"))
                {
                    //绑定
                    await _externalUserService.BindExternalUser(clientId, userId, openId, LoginProvider, context.User.GetDisplayName());
                    context.Response.Redirect(returnUrl);
                    return;
                }
                var externalUserEntity = await _externalUserService.GetExternalUser(clientId, openId);

                if (null == externalUserEntity)
                {
                    await SaveError(context, openId, returnUrl, "账号不存在或未绑定");
                    return;
                    //用户不存在或未绑定
                }

                var userEntity = await _userService.GetUserByIdAsync(externalUserEntity.UserId);

                if (userEntity == null)
                {
                    await SaveError(context, openId, returnUrl, "用户不存在或未注册");
                    return;
                }
                if (userEntity.Enabled == false)
                {
                    await SaveError(context, openId, returnUrl, "您的账号已被禁止登录");
                    return;
                }

                var claims = await _userService.SaveSuccessLoginInfo(clientId, userEntity.Id, context.GetIp(), UserLoginModel.External);
                var idUser = new IdentityServerUser(userEntity.Id)
                {
                    AdditionalClaims = (ICollection<Claim>)claims
                };
                context.Response.Headers.Add("p3p", "CP=\"IDC DSP COR ADM DEVi TAIi PSA PSD IVAi IVDi CONi HIS OUR IND CNT\"");

                await context.SignInAsync(idUser, new AuthenticationProperties { IsPersistent = true, ExpiresUtc = DateTimeOffset.Now.AddHours(2) });
                if (returnUrl.IsEmpty())
                {
                    await context.Response.WriteJsonAsync(new { Error = "缺少ReturnUrl参数" });
                    return;
                }
                context.Response.Redirect(returnUrl);
            }
        }

        private async Task SaveError(HttpContext context, string openId, string returnUrl, string errMsg)
        {
            if (_bindUrl.IsEmpty())
            {
                await context.Response.WriteJsonAsync(new { Error = errMsg });
                return;
            }
            var claims = new[]
            {
                new Claim(CusClaimTypes.ProviderKey,openId),
                new Claim(CusClaimTypes.LoginProvider,LoginProvider)
            };
            var idUser = new IdentityServerUser(openId) { AdditionalClaims = claims };
            context.Response.Headers.Add("p3p", "CP=\"IDC DSP COR ADM DEVi TAIi PSA PSD IVAi IVDi CONi HIS OUR IND CNT\"");
            await context.SignInAsync(idUser, new AuthenticationProperties() { IsPersistent = true, ExpiresUtc = DateTimeOffset.Now.AddHours(2) });
            context.Response.Redirect($"{_bindUrl}{(_bindUrl.Contains("?") ? "&" : "?")}ReturnUrl={HttpUtility.UrlEncode(returnUrl, Encoding.UTF8)}");
        }
    }
}