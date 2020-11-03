using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Fulu.Passport.Domain.Component;
using Fulu.Passport.Domain.Interface;
using FuLu.Passport.Domain.Interface;
using Fulu.Passport.Domain.Models;
using FuLu.Passport.Domain.Options;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Fulu.WebAPI.Abstractions;

namespace Fulu.Passport.API.Controllers
{
    /// <summary>
    /// 短信
    /// </summary>
    [Route("api/[controller]/[action]")]
    [ApiController]
    public class SmsController : ControllerBase
    {
        private readonly IValidationComponent _validationComponent;
        private readonly AppSettings _appSettings;
        private readonly IExternalClient _externalClient;
        /// <summary>
        /// 
        /// </summary>
        public SmsController(IValidationComponent validationComponent, AppSettings appSettings, IExternalClient externalClient)
        {
            _validationComponent = validationComponent;
            _appSettings = appSettings;
            _externalClient = externalClient;
        }

        /// <summary>
        /// 发送短信-未登录状态
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        [AllowAnonymous]
        [ProducesResponseType(typeof(ActionObjectResult), 200)]
        public async Task<ActionResult> Send(SmsSendInputDto inputDto)
        {
            var (response, errMsg) = await _externalClient.CaptchaTicketVerify(inputDto.Ticket, inputDto.RandStr);
            if (response != 1)
                return ObjectResponse.Error(errMsg);

            if (!await _validationComponent.CheckOverLimit(inputDto.Phone))
            {
                return ObjectResponse.Error("请求频繁请稍后再试");
            }

            var smsType = (ValidationType)inputDto.Type;
            var result = await _validationComponent.SendAsync(_appSettings.ClientId, inputDto.Phone, smsType, HttpContext.GetIp());
            return ObjectResponse.Ok(result.Code,result.Message);
        }
    }
}