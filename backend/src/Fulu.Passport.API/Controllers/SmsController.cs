using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Fulu.Passport.Domain.Component;
using Fulu.Passport.Domain.Interface;
using FuLu.Passport.Domain.Interface;
using FuLu.Passport.Domain.Interface.Services;
using Fulu.Passport.Domain.Models;
using FuLu.Passport.Domain.Options;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Fulu.Passport.API.Controllers
{
    /// <summary>
    /// 短信
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    public class SmsController : BaseController
    {
        private readonly IValidationComponent _validationComponent;
        private readonly IUserService _userService;
        private readonly AppSettings _appSettings;
        private readonly IExternalClient _externalClient;
        /// <summary>
        /// 
        /// </summary>
        public SmsController(IValidationComponent validationComponent, IUserService userService, AppSettings appSettings, IExternalClient externalClient)
        {
            _validationComponent = validationComponent;
            _userService = userService;
            _appSettings = appSettings;
            _externalClient = externalClient;
        }
        /// <summary>
        /// 发送短信-未登录状态
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        [AllowAnonymous]
        public async Task<ActionResult> Send(SmsSendInputDto inputDto)
        {
            var (response, errMsg) = await _externalClient.CaptchaTicketVerify(inputDto.Ticket, inputDto.RandStr);
            if (response != 1)
                return Ok("-1", errMsg);

            var smsType = (ValidationType)inputDto.Type;
            if (!await _validationComponent.CheckOverLimit(inputDto.Phone, smsType))
            {
                return Ok("-1", "请求频繁请稍后再试");
            }

            var existPhone = await _userService.ExistPhoneAsync(inputDto.Phone);

            if (smsType == ValidationType.Register || smsType == ValidationType.ChangePhoneNo)
            {
                if (existPhone)
                    return Ok("-1", "该手机号已注册");
            }
            else
            {
                if (!existPhone)
                    return Ok("-1", "该手机号未注册");
            }
            var result = await _validationComponent.SendAsync(_appSettings.ClientId, inputDto.Phone, smsType, HttpContext.GetIp());
            return Ok(result.Code, result.Message);
        }
    }
}