using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using Fulu.Passport.Domain.Component;
using Fulu.WebAPI.Abstractions;
using IdentityServer4;
using IdentityServer4.Configuration;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using FuLu.Passport.Domain.Options;
using FuLu.Passport.Domain.Interface.Services;
using FuLu.Passport.Domain.Models;
using FuLu.Passport.Domain.Entities;
using Fulu.Passport.Domain.Interface;
using Fulu.Passport.Domain.Models;
using FuLu.Passport.Domain.Interface;
using IdentityServer4.Services;

namespace FuLu.IdentityServer.Controllers
{
    /// <summary>
    /// 用户
    /// </summary>
    [Route("api/[controller]/[action]")]
    [ApiController]
    public class UserController : BaseController
    {
        private readonly AppSettings _appSettings;
        private readonly IUserService _userService;
        private readonly UserInteractionOptions _interactionOptions;
        private readonly IValidationComponent _validationComponent;
        private readonly IExternalClient _externalClient;
        private readonly IIdentityServerInteractionService _interaction;
        public UserController(IUserService userService, AppSettings appSettings, UserInteractionOptions interactionOptions, IValidationComponent validationComponent, IExternalClient externalClient, IIdentityServerInteractionService interaction)
        {
            _userService = userService;
            _appSettings = appSettings;
            _interactionOptions = interactionOptions;
            _validationComponent = validationComponent;
            _externalClient = externalClient;
            _interaction = interaction;
        }

        /// <summary>
        /// 登录接口
        /// </summary>
        /// <returns></returns>
        [HttpPost, AllowAnonymous]
        [ProducesResponseType(typeof(DataContent<object>), 200)]
        public async Task<IActionResult> Login(UserLoginInputDto inputDto)
        {
            var (response, errMsg) = await _externalClient.CaptchaTicketVerify(inputDto.Ticket, inputDto.RandStr);
            if (response != 1)
                return Ok("-1", errMsg);

            var result = await _userService.LoginByPasswordAsync(inputDto.UserName, inputDto.Password);

            if (result.Code != "0")
                return Ok(result.Code, result.Message);

            await SignIn(result.Data, UserLoginModel.Password, inputDto.RememberMe);

            return Ok("0", "ok");
        }

        /// <summary>
        /// 短信登录
        /// </summary>
        /// <returns></returns>
        [HttpPost, AllowAnonymous]
        public async Task<IActionResult> LoginBySms(LoginBySmsInputDto inputDto)
        {
            var validSms = await _validationComponent.ValidSmsAsync(inputDto.Phone, inputDto.Code, ValidationType.Login);
            if (!validSms.Data)
                return Ok(validSms.Code, validSms.Message);

            var userEntity = await _userService.GetUserByPhoneAsync(inputDto.Phone);
            if (userEntity == null)
            {
                return Ok("-1", "用户不存在或未注册");
            }
            if (userEntity.Enabled == false)
            {
                return Ok("20043", "您的账号已被禁止登录");
            }

            await SignIn(userEntity, UserLoginModel.SmsCode);
            return Ok("0", "ok");
        }

        /// <summary>
        /// 登出
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        [AllowAnonymous]
        public async Task<IActionResult> SignOut()
        {
            await HttpContext.SignOutAsync(IdentityServerConstants.DefaultCookieAuthenticationScheme);

            return Ok("0", "ok");
        }

        /// <summary>
        /// 登出
        /// </summary>
        /// <returns></returns>
        [HttpGet]
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
            var claims = await _userService.GetLoginClaims(userEntity, _appSettings.ClientId, HttpContext.GetIp(), loginModel);
            var idUser = new IdentityServerUser(userEntity.Id) { AdditionalClaims = (ICollection<Claim>)claims };
            Response.Headers.Add("p3p", "CP=\"IDC DSP COR ADM DEVi TAIi PSA PSD IVAi IVDi CONi HIS OUR IND CNT\"");
            await HttpContext.SignInAsync(idUser, new AuthenticationProperties { IsPersistent = isPersistent, ExpiresUtc = DateTimeOffset.Now.AddHours(2) });
        }

        /// <summary>
        /// 
        /// </summary>
        [HttpGet, AllowAnonymous]
        public async Task<IActionResult> GetErrorMsg(string errorId)
        {
            var err = await _interaction.GetErrorContextAsync(errorId);
            if (err == null)
            {
                return Ok("0", "未知错误");
            }
            return Ok("0", err.ErrorDescription);
        }

    }
}
