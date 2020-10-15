using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Fulu.Passport.Domain.Component;
using Fulu.Passport.Domain.Interface;
using Fulu.Passport.Domain.Interface.Services;
using Fulu.Passport.Domain.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Fulu.BouncyCastle;
using Fulu.Passport.Domain;
using FuLu.Passport.Domain.Options;
using FuLu.Passport.Domain.Interface.Services;
using Microsoft.AspNetCore.Http;
using IdentityModel;

namespace Fulu.Passport.API.Controllers
{
    /// <summary>
    /// 用户
    /// </summary>
    [Route("api/[controller]/[action]")]
    [ApiController]
    public class UserController : BaseController
    {
        private readonly AppSettings _appSettings;
        private readonly IEncryptService _encryptService;
        private readonly IValidationComponent _smsComponent;
        private readonly IUserService _userService;
        /// <summary>
        /// 
        /// </summary>
        /// <param name="encryptService"></param>
        /// <param name="userService"></param>
        /// <param name="appSettings"></param>
        /// <param name="smsComponent"></param>
        public UserController(IEncryptService encryptService, IUserService userService, AppSettings appSettings, IValidationComponent smsComponent)
        {
            _encryptService = encryptService;
            _userService = userService;
            _appSettings = appSettings;
            _smsComponent = smsComponent;
        }

        /// <summary>
        /// 注册接口
        /// </summary>
        /// <returns></returns>
        [HttpPost, AllowAnonymous]
        public async Task<IActionResult> Register(RegisterInputDto inputDto)
        {
            var password = _encryptService.DecryptRsa(inputDto.Password);

            if (string.IsNullOrEmpty(password))
                return Ok("-1", "密码格式不正确");

            if (Regex.IsMatch(password, RegexConstance.IsPassword) == false)
            {
                return Ok(password.Length < 6 ? ("20002", "密码长度不能少于6个字符") : ("20002", "密码长度不能超过20个字符"));
            }

            //验证验证码
            var validResult = await _smsComponent.ValidSmsAsync(inputDto.Phone, inputDto.Code, ValidationType.Register);
            if (!validResult.Data)
            {
                return Ok(validResult.Code, validResult.Message);
            }

            if (await _userService.ExistPhoneAsync(inputDto.Phone))
                return Ok("-1", "手机号已存在");

            var userEntity = await _userService.RegisterAsync(inputDto.Phone, password, _appSettings.ClientId, _appSettings.ClientName, HttpContext.GetIp(), inputDto.Nickname);

            var openId = MD5.Compute($"openId{_appSettings.ClientId}{userEntity.Id}");

            return Ok(new { OpenId = openId ?? "", UserName = userEntity.UserName ?? "", NickName = userEntity.NickName ?? "", Phone = userEntity.Phone ?? "", Gender = userEntity.Gender, Email = userEntity.Email ?? "", FigureUrl = userEntity.FigureUrl ?? "" });
        }

        /// <summary>
        ///忘记密码-重置密码
        /// </summary>
        /// <returns></returns>
        [HttpPost, AllowAnonymous]
        public async Task<ActionResult> ResetPassword(ResetPasswordInputDto inputDto)
        {
            var password = _encryptService.DecryptRsa(inputDto.Password);

            if (string.IsNullOrEmpty(password))
                return Ok("-1", "密码格式不正确");

            if (Regex.IsMatch(password, RegexConstance.IsPassword) == false)
                return Ok(password.Length < 6 ? ("20002", "密码长度不能少于6个字符") : ("20002", "密码长度不能超过20个字符"));

            var cacheTicket = await _smsComponent.GetValidateTicketAsync(inputDto.Code);
            if (cacheTicket == null)
                return Ok(false, "-1", "验证无效或验证已超时");

            var phoneExists = await _userService.ExistPhoneAsync(cacheTicket.Phone);
            if (!phoneExists)
                return Ok("-1", "该用户不存在");

            await _userService.ResetPasswordAsync(cacheTicket.Phone, password);

            return Ok("0", "ok");
        }

        /// <summary>
        /// 忘记密码-验证
        /// </summary>
        /// <returns></returns>
        [HttpPost, AllowAnonymous]
        public async Task<ActionResult> ResetPasswordValidate(ResetPasswordValidateInputDto inputDto)
        {
            var validSms = await _smsComponent.ValidSmsAsync(inputDto.Phone, inputDto.Code, ValidationType.ResetPassword);

            if (!validSms.Data)
                return Ok(validSms);

            if (!await _userService.ExistPhoneAsync(inputDto.Phone))
            {
                return Ok("-1", "手机号不存在");
            }

            var ticket = await _smsComponent.CreateTicketAsync(inputDto.Phone, ValidationType.ResetPassword);

            return Ok(ticket);
        }


        /// <summary>
        /// 获取用户信息
        /// </summary>
        /// <returns></returns>
        [HttpGet, Authorize]
        public async Task<ActionResult> GetUserInfo()
        {
            var account = await _userService.GetUserByPhoneAsync(User.GetPhoneNo());

            var nickName = account.NickName;
            var userId = account.Id;
            var openId = User.GetOpenId();
            var phone = account.Phone;
            phone = Regex.Replace(phone, "(\\d{3})\\d{4}(\\d{4})", "$1****$2");
            return Ok(new
            {
                userid = userId,
                openid = openId,
                nickname = nickName,
                cellphone = phone
            });
        }


    }
}