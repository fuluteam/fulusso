using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using AutoMapper;
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
using Fulu.WebAPI.Abstractions;

namespace Fulu.Passport.API.Controllers
{
    /// <summary>
    /// 用户
    /// </summary>
    [Route("api/[controller]/[action]")]
    [ApiController]
    [Authorize]
    public class UserController : ControllerBase
    {
        private readonly AppSettings _appSettings;
        private readonly IEncryptService _encryptService;
        private readonly IValidationComponent _validationComponent;
        private readonly IUserService _userService;
        private readonly IMapper _mapper;
        /// <summary>
        /// 
        /// </summary>
        public UserController(IEncryptService encryptService, IUserService userService, AppSettings appSettings, IValidationComponent validationComponent, IMapper mapper)
        {
            _encryptService = encryptService;
            _userService = userService;
            _appSettings = appSettings;
            _validationComponent = validationComponent;
            _mapper = mapper;
        }

        /// <summary>
        /// 注册接口
        /// </summary>
        /// <returns></returns>
        [HttpPost, AllowAnonymous]
        [ProducesResponseType(typeof(ActionObjectResult), 200)]
        public async Task<IActionResult> Register(RegisterInputDto inputDto)
        {
            var password = _encryptService.DecryptRsa(inputDto.Password);

            if (string.IsNullOrEmpty(password))
                return ObjectResponse.Error("密码格式不正确");

            if (Regex.IsMatch(password, RegexConstance.IsPassword) == false)
            {
                return ObjectResponse.Error(password.Length < 6 ? "密码长度不能少于6个字符" : "密码长度不能超过20个字符");
            }

            //验证验证码
            var validResult = await _validationComponent.ValidSmsAsync(inputDto.Phone, inputDto.Code);
            if (!validResult.Data)
            {
                return ObjectResponse.Ok(validResult.Code,validResult.Message);
            }

            if (await _userService.ExistPhoneAsync(inputDto.Phone))
                return ObjectResponse.Error("手机号已存在");

            var userEntity = await _userService.RegisterAsync(inputDto.Phone, password, _appSettings.ClientId, _appSettings.ClientName, HttpContext.GetIp(), inputDto.Nickname);

            var openId = MD5.Compute($"openId{_appSettings.ClientId}{userEntity.Id}");

            var registerOutput = _mapper.Map<RegisterOutputDto>(userEntity);

            registerOutput.OpenId = openId;

            return ObjectResponse.Ok(registerOutput);
        }

        /// <summary>
        ///忘记密码-重置密码
        /// </summary>
        /// <returns></returns>
        [HttpPost, AllowAnonymous]
        [ProducesResponseType(typeof(ActionObjectResult<RegisterOutputDto>), 200)]
        public async Task<ActionResult> ResetPassword(ResetPasswordInputDto inputDto)
        {
            var password = _encryptService.DecryptRsa(inputDto.Password);

            if (string.IsNullOrEmpty(password))
                return ObjectResponse.Error("密码格式不正确");

            if (Regex.IsMatch(password, RegexConstance.IsPassword) == false)
                return ObjectResponse.Error(password.Length < 6 ? "密码长度不能少于6个字符" : "密码长度不能超过20个字符");

            var phone = await _validationComponent.GetTicketPhoneAsync(inputDto.Ticket);
            if (string.IsNullOrWhiteSpace(phone))
                return ObjectResponse.Error("身份验证无效或验证已过期");

            var phoneExists = await _userService.ExistPhoneAsync(phone);
            if (!phoneExists)
                return ObjectResponse.Error("该用户不存在");

            await _userService.ResetPasswordAsync(phone, password);

            return ObjectResponse.Ok();
        }

        /// <summary>
        /// 忘记密码-验证
        /// </summary>
        /// <returns></returns>
        [HttpPost, AllowAnonymous]
        [ProducesResponseType(typeof(ActionObjectResult<string>), 200)]
        public async Task<ActionResult> ResetPasswordValidate(ResetPasswordValidateInputDto inputDto)
        {
            var validSms = await _validationComponent.ValidSmsAsync(inputDto.Phone, inputDto.Code);

            if (!validSms.Data)
                return ObjectResponse.Ok(validSms.Code,validSms.Message);

            if (!await _userService.ExistPhoneAsync(inputDto.Phone))
                return ObjectResponse.Ok("手机号不存在");

            var ticket = await _validationComponent.CreateTicketAsync(inputDto.Phone);

            return ObjectResponse.Ok(data: ticket);
        }

        /// <summary>
        /// 身份验证
        /// </summary>
        /// <returns></returns>
        [HttpPost, AllowAnonymous]
        [ProducesResponseType(typeof(ActionObjectResult<string>), 200)]
        public async Task<IActionResult> ValidateIdentity(IdentityValidateInputDto inputDto)
        {
            var validSms = await _validationComponent.ValidSmsAsync(inputDto.Phone, inputDto.Code);

            if (!validSms.Data)
                return ObjectResponse.Ok(validSms.Code,validSms.Message);

            if (!await _userService.ExistPhoneAsync(inputDto.Phone))
                return ObjectResponse.Error("手机号不存在");

            var ticket = await _validationComponent.CreateTicketAsync(inputDto.Phone);

            return ObjectResponse.Ok(data: ticket);
        }

        /// <summary>
        /// 获取用户信息
        /// </summary>
        /// <returns></returns>
        [HttpGet, Authorize]
        [ProducesResponseType(typeof(ActionObjectResult<GetUserInfoOutput>), 200)]
        public async Task<ActionResult> GetUserInfo()
        {
            var userEntity = await _userService.GetUserByPhoneAsync(User.GetPhoneNo());

            var nickName = userEntity.NickName;
            var userId = userEntity.Id;
            var openId = User.GetOpenId();
            var phone = Regex.Replace(userEntity.Phone, "(\\d{3})\\d{4}(\\d{4})", "$1****$2");

            var userInfoOutput = new GetUserInfoOutput
            {
                UserId = userId,
                OpenId = openId,
                NickName = nickName,
                Phone = phone
            };
            return ObjectResponse.Ok(userInfoOutput);
        }

        /// <summary>
        /// 修改手机号
        /// </summary>
        /// <returns></returns>
        [HttpPost, AllowAnonymous]
        [ProducesResponseType(typeof(ActionObjectResult), 200)]
        public async Task<ActionResult> ChangePhone(ChangePhoneInputDto inputDto)
        {
            var phone = await _validationComponent.GetTicketPhoneAsync(inputDto.Code);
            if (string.IsNullOrWhiteSpace(phone))
                return ObjectResponse.Error("身份验证无效或验证已过期");

            if (inputDto.Phone == phone)
                return ObjectResponse.Error("输入手机号与原手机号相同");

            var dataContent = await _validationComponent.ValidSmsAsync(inputDto.Phone, inputDto.Code);

            if (!dataContent.Data)
                return ObjectResponse.Ok(dataContent.Code, dataContent.Message);

            var result = await _userService.ChangePhoneAsync(phone, inputDto.Phone);
            return ObjectResponse.Ok(result);
        }
    }
}