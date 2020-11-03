using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Fulu.Core;
using Fulu.Core.Extensions;
using FuLu.Passport.Domain.Entities;
using Fulu.Passport.Domain.Interface;
using FuLu.Passport.Domain.Interface;
using Fulu.Passport.Domain.Interface.Services;
using FuLu.Passport.Domain.Interface.Services;
using Fulu.Passport.Domain.Models;
using FuLu.Passport.Domain.Models;
using FuLu.Passport.Domain.Options;
using Fulu.WebAPI.Abstractions;
using IdentityModel;
using IdentityServer4;
using IdentityServer4.Configuration;
using IdentityServer4.Services;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Fulu.Passport.Web.Controllers
{
    /// <summary>
    /// 用户
    /// </summary>
    [Route("api/[controller]/[action]")]
    [ApiController]
    public class UserController : ControllerBase
    {
        private readonly AppSettings _appSettings;
        private readonly IUserService _userService;
        private readonly UserInteractionOptions _interactionOptions;
        private readonly IValidationComponent _validationComponent;
        private readonly IExternalClient _externalClient;
        private readonly IIdentityServerInteractionService _interaction;
        private readonly IExternalUserService _externalUserService;
        public UserController(IUserService userService, AppSettings appSettings, UserInteractionOptions interactionOptions, IValidationComponent validationComponent, IExternalClient externalClient, IIdentityServerInteractionService interaction, IExternalUserService externalUserService)
        {
            _userService = userService;
            _appSettings = appSettings;
            _interactionOptions = interactionOptions;
            _validationComponent = validationComponent;
            _externalClient = externalClient;
            _interaction = interaction;
            _externalUserService = externalUserService;
        }

        /// <summary>
        /// 登录接口
        /// </summary>
        /// <returns></returns>
        [HttpPost, AllowAnonymous]
        [ProducesResponseType(typeof(ActionObjectResult), 200)]
        public async Task<IActionResult> Login(UserLoginInputDto inputDto)
        {
            var (response, errMsg) = await _externalClient.CaptchaTicketVerify(inputDto.Ticket, inputDto.RandStr);
            if (response != 1)
                return ObjectResponse.Ok(-1, errMsg);

            var result = await _userService.LoginByPasswordAsync(inputDto.UserName, inputDto.Password);

            if (result.Code != 0)
                return ObjectResponse.Ok(result.Code, result.Message);

            await SignIn(result.Data, UserLoginModel.Password, inputDto.RememberMe);

            return ObjectResponse.Ok();
        }

        /// <summary>
        /// 短信登录
        /// </summary>
        /// <returns></returns>
        [HttpPost, AllowAnonymous]
        [ProducesResponseType(typeof(ActionObjectResult), 200)]
        public async Task<IActionResult> LoginBySms(LoginBySmsInputDto inputDto)
        {
            var validSms = await _validationComponent.ValidSmsAsync(inputDto.Phone, inputDto.Code);
            if (!validSms.Data)
                return ObjectResponse.Ok(validSms.Code, validSms.Message);

            var userEntity = await _userService.GetUserByPhoneAsync(inputDto.Phone);
            if (userEntity == null)
            {
                return ObjectResponse.Ok(-1, "用户不存在或未注册");
            }
            if (userEntity.Enabled == false)
            {
                return ObjectResponse.Ok(20043, "您的账号已被禁止登录");
            }
            await SignIn(userEntity, UserLoginModel.SmsCode);
            return ObjectResponse.Ok();
        }

        /// <summary>
        /// 登出
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        [AllowAnonymous]
        [ProducesResponseType(typeof(ActionObjectResult), 200)]
        public async Task<IActionResult> SignOut()
        {
            await HttpContext.SignOutAsync(IdentityServerConstants.DefaultCookieAuthenticationScheme);

            return ObjectResponse.Ok();
        }

        /// <summary>
        /// 登出
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [ProducesResponseType(typeof(ActionObjectResult<string>), 200)]
        public async Task<IActionResult> Logout(string returnUrl)
        {
            await HttpContext.SignOutAsync(IdentityServerConstants.DefaultCookieAuthenticationScheme);

            if (string.IsNullOrWhiteSpace(returnUrl))
            {
                returnUrl = _interactionOptions.LoginUrl;
            }
            return new RedirectResult(returnUrl);
        }

        private async Task SignIn(UserEntity userEntity, UserLoginModel loginModel, bool isPersistent = true)
        {
            var claims = await _userService.SaveSuccessLoginInfo(_appSettings.ClientId, userEntity.Id, HttpContext.GetIp(), loginModel);

            var idUser = new IdentityServerUser(userEntity.Id) { AdditionalClaims = (ICollection<Claim>)claims };
            Response.Headers.Add("p3p", "CP=\"IDC DSP COR ADM DEVi TAIi PSA PSD IVAi IVDi CONi HIS OUR IND CNT\"");
            await HttpContext.SignInAsync(idUser, new AuthenticationProperties { IsPersistent = isPersistent, ExpiresUtc = DateTimeOffset.Now.AddHours(2) });
        }

        /// <summary>
        /// 
        /// </summary>
        [HttpGet, AllowAnonymous]
        [ProducesResponseType(typeof(ActionObjectResult), 200)]
        public async Task<IActionResult> GetErrorMsg(string errorId)
        {
            var err = await _interaction.GetErrorContextAsync(errorId);
            if (err == null)
            {
                return ObjectResponse.Ok(0, "未知错误");
            }
            return ObjectResponse.Ok(0, err.ErrorDescription);
        }

        /// <summary>
        /// 短信登录绑定
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<IActionResult> LoginByCodeBind(LoginByCodeBindModel model)
        {
            if (!User.Claims.Any())
                return ObjectResponse.Ok(10026, "操作超时，请重新授权");
            var loginProvider = User.Claims.FirstOrDefault(x => x.Type == CusClaimTypes.LoginProvider)?.Value;
            var providerKey = User.Claims.FirstOrDefault(x => x.Type == CusClaimTypes.ProviderKey)?.Value;
            if (loginProvider.IsEmpty() || providerKey.IsEmpty())
                return ObjectResponse.Ok(10026, "操作超时，请重新授权");

            var phoneExist = await _userService.ExistPhoneAsync(model.Phone);


            var validResult = await _validationComponent.ValidSmsAsync(model.Phone, model.Code);
            //验证验证码
            if (!validResult.Data)
                return ObjectResponse.Ok(20009, "验证码不正确");

            if (!string.IsNullOrEmpty(model.Code))
                await _validationComponent.ClearSession(model.Phone);

            if (!phoneExist)
            {//注册用户
                var password = Str.GetRandomString(10);
                await _userService.RegisterAsync(model.Phone, password, _appSettings.ClientId, _appSettings.ClientName, HttpContext.GetIp());
            }

            var account = await _userService.GetUserByPhoneAsync(model.Phone);
            if (null == account)
                return ObjectResponse.Ok(20041, "该用户不存在");

            if (!account.Enabled)
                return ObjectResponse.Ok(20043, "您的账号已被禁用");

            //绑定
            var bindResult = await _externalUserService.BindExternalUser(_appSettings.ClientId, account.Id, providerKey, loginProvider, string.Empty);
            if (bindResult.Code != 0)
            {
                return ObjectResponse.Ok(bindResult.Code, bindResult.Message);
            }
            await SignIn(account, UserLoginModel.External);
            return ObjectResponse.Ok();
        }

        /// <summary>
        /// 密码登录绑定
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> LoginByPassBind(LoginByPassBindModel model)
        {
            if (!User.Claims.Any())
                return ObjectResponse.Ok(10026, "操作超时，请重新授权");
            var loginProvider = User.Claims.FirstOrDefault(x => x.Type == CusClaimTypes.LoginProvider)?.Value;
            var providerKey = User.Claims.FirstOrDefault(x => x.Type == CusClaimTypes.ProviderKey)?.Value;
            if (loginProvider.IsEmpty() || providerKey.IsEmpty())
                return ObjectResponse.Ok(10026, "操作超时，请重新授权");

            var loginResult = await _userService.LoginByPasswordAsync(model.UserName, model.Password);
            if (loginResult.Code != 0)
            {
                return ObjectResponse.Ok(loginResult.Code, loginResult.Message);
            }
            await SignIn(loginResult.Data, UserLoginModel.External);
            return ObjectResponse.Ok();
        }
    }
}
