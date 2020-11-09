using System;
using System.Threading.Tasks;
using Fulu.Core.Extensions;
using Fulu.Passport.Domain.Interface.Services;
using FuLu.IdentityServer.Stores;
using Fulu.Passport.Domain.Interface.CacheStrategy;
using FuLu.Passport.Domain.Interface.Services;
using FuLu.Passport.Domain.Models;
using IdentityServer4.Models;
using IdentityServer4.Validation;
using Microsoft.AspNetCore.Http;

namespace Fulu.Passport.Web.Validator
{
    public class ExternalGrantValidator : IExtensionGrantValidator
    {
        private readonly IExternalUserCacheStrategy _userExternalService;
        private readonly IUserService _userService;
        private readonly IHttpContextAccessor _contextAccessor;
        public ExternalGrantValidator(IExternalUserCacheStrategy userExternalService, IUserService userService, IHttpContextAccessor contextAccessor)
        {
            _userExternalService = userExternalService;
            _userService = userService;
            _contextAccessor = contextAccessor;
            GrantType = CustomGrantType.External;
        }
        public string GrantType { get; }

        public async Task ValidateAsync(ExtensionGrantValidationContext context)
        {
            var providerKey = context.Request.Raw.Get("provider_key");
            var loginProvider = context.Request.Raw.Get("login_provider");
            if (string.IsNullOrEmpty(providerKey))
            {
                context.Result = new GrantValidationResult(TokenRequestErrors.InvalidRequest, "provider_key is not valid");
                return;
            }

            if (string.IsNullOrEmpty(loginProvider))
            {
                context.Result = new GrantValidationResult(TokenRequestErrors.InvalidRequest, "login_provider is not valid");
                return;
            }
            try
            {
                var userExternalEntity = await _userExternalService.GetExternalUser(context.Request.ClientId.ToInt32(), providerKey);
                if (userExternalEntity == null)
                {
                    context.Result = new GrantValidationResult(TokenRequestErrors.InvalidRequest, "账号未绑定");
                    return;
                }

                var userEntity = await _userService.GetUserByIdAsync(userExternalEntity.UserId);

                if (userEntity == null)
                {
                    context.Result = new GrantValidationResult(TokenRequestErrors.InvalidRequest, "用户不存在或未注册");
                    return;
                }

                if (!userEntity.Enabled)
                {
                    context.Result = new GrantValidationResult(TokenRequestErrors.InvalidRequest, "您的账号已被禁用");
                    return;
                }

                var claims = await _userService.SaveSuccessLoginInfo(context.Request.ClientId.ToInt32(), userEntity.Id, _contextAccessor.HttpContext.GetIp(), UserLoginModel.External);

                context.Result = new GrantValidationResult(userEntity.UserName, "custom", claims);
            }
            catch (Exception ex)
            {
                context.Result = new GrantValidationResult(TokenRequestErrors.InvalidRequest, ex.Message);
            }
        }
    }
}
