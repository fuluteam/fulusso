using System;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Fulu.Core.Extensions;
using FuLu.IdentityServer.Stores;
using Fulu.Passport.Domain;
using Fulu.Passport.Domain.Interface;
using FuLu.Passport.Domain.Interface.Services;
using FuLu.Passport.Domain.Models;
using IdentityServer4.Models;
using IdentityServer4.Validation;
using Microsoft.AspNetCore.Http;

namespace Fulu.Passport.Web.Validator
{
    public class SmsGrantValidator : IExtensionGrantValidator
    {
        private readonly IHttpContextAccessor _contextAccessor;
        private readonly IValidationComponent _validationComponent;
        private readonly IUserService _userService;

        public SmsGrantValidator(IHttpContextAccessor contextAccessor, IValidationComponent validationComponent, IUserService userService)
        {
            _contextAccessor = contextAccessor;
            _validationComponent = validationComponent;
            _userService = userService;
            GrantType = CustomGrantType.Sms;
        }

        public async Task ValidateAsync(ExtensionGrantValidationContext context)
        {
            var phone = context.Request.Raw.Get("phone");
            var code = context.Request.Raw.Get("code");
            if (string.IsNullOrEmpty(phone) || Regex.IsMatch(phone, RegexConstance.IsPhone) == false)
            {
                context.Result = new GrantValidationResult(TokenRequestErrors.InvalidRequest, "phone is not valid");
                return;
            }

            if (string.IsNullOrEmpty(code))
            {
                context.Result = new GrantValidationResult(TokenRequestErrors.InvalidRequest, "code is not valid");
                return;
            }

            try
            {
                var validSms = await _validationComponent.ValidSmsAsync(phone, code);
                if (!validSms.Data)
                {
                    context.Result = new GrantValidationResult(TokenRequestErrors.InvalidRequest, validSms.Message);
                    return;
                }

                var userEntity = await _userService.GetUserByPhoneAsync(phone);
                if (userEntity == null)
                {
                    context.Result = new GrantValidationResult(TokenRequestErrors.InvalidRequest, "用户不存在或未注册");
                    return;
                }
                if (userEntity.Enabled == false)
                {
                    context.Result = new GrantValidationResult(TokenRequestErrors.InvalidRequest, "您的账号已被禁止登录");
                    return;
                }

                await _userService.SaveSuccessLoginInfo(context.Request.ClientId.ToInt32(), userEntity.Id, _contextAccessor.HttpContext.GetIp(),
                     UserLoginModel.SmsCode);
            }
            catch (Exception ex)
            {
                context.Result = new GrantValidationResult(TokenRequestErrors.InvalidRequest, ex.Message);
            }
        }

        public string GrantType { get; }
    }
}
