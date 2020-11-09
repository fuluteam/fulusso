using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using Fulu.Core;
using Fulu.Core.Extensions;
using Fulu.Passport.Domain.Entities;
using FuLu.Passport.Domain.Entities;
using Fulu.Passport.Domain.Interface;
using FuLu.Passport.Domain.Interface;
using Fulu.Passport.Domain.Interface.CacheStrategy;
using FuLu.Passport.Domain.Interface.Services;
using Fulu.Passport.Domain.Models;
using FuLu.Passport.Domain.Models;
using FuLu.Passport.Domain.Options;
using Fulu.WebAPI.Abstractions;
using IdentityModel;
using IdentityServer4;
using IdentityServer4.Configuration;
using IdentityServer4.Extensions;
using IdentityServer4.Models;
using IdentityServer4.Services;
using IdentityServer4.Stores;
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
    public class UserController : Controller
    {
        private readonly AppSettings _appSettings;
        private readonly IUserService _userService;
        private readonly Domain.Options.Endpoints _endpoints;
        private readonly UserInteractionOptions _interactionOptions;
        private readonly IValidationComponent _validationComponent;
        private readonly IExternalClient _externalClient;
        private readonly IIdentityServerInteractionService _interaction;
        private readonly IExternalUserCacheStrategy _externalUserService;
        private readonly IMessageStore<ErrorMessage> _errorMessageStore;
        public UserController(IUserService userService, AppSettings appSettings, UserInteractionOptions interactionOptions, IValidationComponent validationComponent, IExternalClient externalClient, IIdentityServerInteractionService interaction, IExternalUserCacheStrategy externalUserService, Domain.Options.Endpoints endpoints, IMessageStore<ErrorMessage> errorMessageStore)
        {
            _userService = userService;
            _appSettings = appSettings;
            _interactionOptions = interactionOptions;
            _validationComponent = validationComponent;
            _externalClient = externalClient;
            _interaction = interaction;
            _externalUserService = externalUserService;
            _endpoints = endpoints;
            _errorMessageStore = errorMessageStore;
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
        [HttpPost, AllowAnonymous]
        public async Task<IActionResult> LoginByCodeBind(LoginByCodeBindModel model)
        {
            var result = await HttpContext.AuthenticateAsync(IdentityServerConstants.ExternalCookieAuthenticationScheme);
            if (result.Succeeded == false)
            {
                return ObjectResponse.Ok(10026, "External authentication error");
            }

            var (loginProvider, providerKey) = GetExternalUser(result);


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
            await HttpContext.SignOutAsync(IdentityServerConstants.ExternalCookieAuthenticationScheme);
            await SignIn(account, UserLoginModel.External);
            return ObjectResponse.Ok();
        }

        /// <summary>
        /// 密码登录绑定
        /// </summary>
        [HttpPost, AllowAnonymous]
        public async Task<IActionResult> LoginByPassBind(LoginByPassBindModel model)
        {
            var result = await HttpContext.AuthenticateAsync(IdentityServerConstants.ExternalCookieAuthenticationScheme);
            if (result.Succeeded == false)
            {
                return ObjectResponse.Ok(10026, "External authentication error");
            }

            var (loginProvider, providerKey) = GetExternalUser(result);

            if (loginProvider.IsEmpty() || providerKey.IsEmpty())
                return ObjectResponse.Ok(10026, "操作超时，请重新授权");

            var loginResult = await _userService.LoginByPasswordAsync(model.UserName, model.Password);
            if (loginResult.Code != 0)
            {
                return ObjectResponse.Ok(loginResult.Code, loginResult.Message);
            }
            await HttpContext.SignOutAsync(IdentityServerConstants.ExternalCookieAuthenticationScheme);
            await SignIn(loginResult.Data, UserLoginModel.External);
            return ObjectResponse.Ok();
        }

        /// <summary>
        /// 外部账号登录
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpGet, AllowAnonymous]
        public IActionResult ExternalLogin([FromQuery] ExternalLoginModel model)
        {
            var authenticationProperties = new AuthenticationProperties()
            {
                RedirectUri = Url.Action(nameof(ExternalLoginCallback)),
                Items =
                {
                    { "returnUrl", model.ReturnUrl },
                    { "scheme", model.Provider },
                }
            };

            return Challenge(authenticationProperties, model.Provider);
        }

        /// <summary>
        /// 外部登录回调
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> ExternalLoginCallback()
        {
            var result = await HttpContext.AuthenticateAsync(IdentityServerConstants.ExternalCookieAuthenticationScheme);

            var returnUrl = result.Properties.Items["returnUrl"];

            if (result.Succeeded == false)
            {
                return await RedirectErrorResult("error", "External authentication error", returnUrl);
            }

            var (loginProvider, providerKey) = GetExternalUser(result);

            var externalUserEntity = await _externalUserService.GetExternalUser(_appSettings.ClientId, providerKey);

            UserEntity userEntity;

            if (externalUserEntity == null)
            {//1.第三方用户未绑定
                if (!HttpContext.User.Identity.IsAuthenticated) //用户未登录，引导用户绑定
                {
                    var bindUrl = _endpoints.BindUrl;
                    if (!string.IsNullOrEmpty(bindUrl))
                        bindUrl = $"{bindUrl}?ReturnUrl={HttpUtility.UrlEncode(returnUrl, Encoding.UTF8)}";
                    return Redirect(bindUrl);
                }

                //用户已登录，直接绑定
                var userId = HttpContext.User.GetUserId();
                userEntity = await _userService.GetUserByIdAsync(userId);

                if (userEntity != null)
                    await _externalUserService.BindExternalUser(_appSettings.ClientId, userEntity.Id,
                        providerKey, loginProvider, string.Empty);

                return Redirect(_endpoints.CenterUrl);
            }

            //2.第三方用户已绑定

            if (HttpContext.User.Identity.IsAuthenticated)
            {//用户已登录
                if (User.GetUserId() == externalUserEntity.UserId)
                    return Redirect(returnUrl);
                return await RedirectErrorResult("绑定失败", "已绑定过其他通行证账号", returnUrl);
            }

            //用户未登录，获取用户进行登录
            userEntity = await _userService.GetUserByIdAsync(externalUserEntity.UserId);

            if (userEntity == null)
            {
                return await RedirectErrorResult("登录失败", "用户不存在或未注册", returnUrl);
            }

            if (userEntity.Enabled == false)
            {
                return await RedirectErrorResult("登录失败", "您的账号已被禁止登录", returnUrl);
            }

            await HttpContext.SignOutAsync(IdentityServerConstants.ExternalCookieAuthenticationScheme);
            await SignIn(userEntity, UserLoginModel.External);

            return Redirect(returnUrl);
        }

        private (string loginProvider, string providerKey) GetExternalUser(AuthenticateResult result)
        {
            var externalUser = result.Principal;
            var userIdClaim = externalUser.FindFirst(JwtClaimTypes.Subject) ??
                              externalUser.FindFirst(ClaimTypes.NameIdentifier) ??
                              throw new Exception("Unknown userid");
            var loginProvider = result.Properties.Items["scheme"];
            var providerKey = userIdClaim.Value;

            return (loginProvider, providerKey);
        }

        private async Task<RedirectResult> RedirectErrorResult(string error, string errorDescription, string redirectUri)
        {
            var msg = new Message<ErrorMessage>(new ErrorMessage
            {
                Error = error,
                ErrorDescription = errorDescription,
                RedirectUri = redirectUri
            }, DateTime.UtcNow);
            var id = await _errorMessageStore.WriteAsync(msg);
            return Redirect($"{_interactionOptions.ErrorUrl}?errorId={id}");
        }
    }
}
