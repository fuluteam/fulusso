using System;
using System.Data;
using System.Net.Http;
using System.Threading.Tasks;
using Fulu.Passport.Domain.Entities;
using Fulu.Passport.Domain.Interface;
using FuLu.Passport.Domain.Interface;
using Fulu.Passport.Domain.Interface.Repositories;
using Fulu.Passport.Domain.Models;
using Fulu.Passport.Domain.Options;
using FuLu.Passport.Domain.Options;
using Fulu.WebAPI.Abstractions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Redis;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using ICH.TransferJob;

namespace Fulu.Passport.Domain.Component
{
    /// <summary>
    /// 
    /// </summary>
    public class ValidationComponent : ResultBase, IValidationComponent
    {
        private readonly IRedisCache _redisCache;
        private readonly SmsOptions _smsServerOptions;
        private readonly IPassportClient _passportClient;
        private readonly IUnitOfWork _unitOfWork;
        private readonly ISmsRecordRepository _smsRecordRepository;
        private readonly IBackgroundRunService _backgroundRunService;
        private readonly AppSettings _appSettings;
        /// <summary>
        /// 
        /// </summary>
        public ValidationComponent(IRedisCache redisCache,
            IOptions<SmsOptions> smsOptions, IPassportClient passportClient, ISmsRecordRepository smsRecordRepository, IUnitOfWork unitOfWork, IBackgroundRunService backgroundRunService, AppSettings appSettings)
        {
            _redisCache = redisCache;
            _passportClient = passportClient;
            _smsRecordRepository = smsRecordRepository;
            _unitOfWork = unitOfWork;
            _backgroundRunService = backgroundRunService;
            _appSettings = appSettings;
            _smsServerOptions = smsOptions.Value;
        }

        private static string GetSmsKey(string phone, ValidationType type)
        {
            return $"{(int)type}_{phone}";
        }

        private static string GetTicketKey(string phone, ValidationType type)
        {
            return $"ticket_{(int)type}_{phone}";
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="appId"></param>
        /// <param name="content"></param>
        /// <param name="type"></param>
        /// <param name="phone"></param>
        /// <param name="ip"></param>
        /// <param name="isSuccess"></param>
        /// <param name="code"></param>
        /// <param name="expiresMinute"></param>
        /// <param name="smsMsgId"></param>
        /// <param name="smsMsg"></param>
        /// <returns></returns>
        public async Task SaveLog(int appId, string content, ValidationType type, string phone, string ip,
            bool isSuccess, string code, int expiresMinute, string smsMsgId, string smsMsg)
        {
            var smsRecordEntity = new SmsRecordEntity
            {
                SendTime = DateTime.Now,
                ClientId = appId,
                Content = content,
                SendType = type.ToString(),
                Receiver = phone,
                IpAddress = ip,
                SendResult = isSuccess,
                Remark = isSuccess ? $"发送成功，msg_id：{smsMsgId}" : $"发送失败，错误描述：{smsMsg}，msg_id：{smsMsgId}"
            };
            await _smsRecordRepository.InsertAsync(smsRecordEntity);
            await _unitOfWork.SaveChangesAsync();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="phone"></param>
        /// <param name="code"></param>
        /// <param name="type"></param>
        /// <returns></returns>
        public async Task<DataContent<bool>> ValidSmsAsync(string phone, string code, ValidationType type)
        {
            var uniqueKey = GetSmsKey(phone, type);
            var smsCache = await _redisCache.GetAsync<SmsCache>(uniqueKey);

            if (smsCache == null)
                return Ok(false, "-1", "未发送验证码或验证码已超时");

            if (smsCache.ValidateCount >= 3)  //0 1  2
                return Ok(false, "-1", "输入错误的次数超过3次，请重新获取验证码");
            if (code == smsCache.Code)
            {
                await _redisCache.KeyDeleteAsync(uniqueKey);
                return Ok(true);
            }

            var timeSpan = smsCache.StartTime.AddMinutes(smsCache.ExpiresMinute) - DateTime.Now;
            if (timeSpan.TotalSeconds <= 0)
                return Ok(false, "-1", "未发送验证码或验证码已超时");
            smsCache.ValidateCount += 1;   //更新验证次数，不延长时间  
            await _redisCache.AddAsync(uniqueKey, smsCache, timeSpan);
            return Ok(false, "-1", "输入验证码不正确");
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="phone"></param>
        /// <param name="type"></param>
        /// <returns></returns>
        public async Task ClearSession(string phone, ValidationType type)
        {
            var uniqueKey = GetSmsKey(phone, type);
            await _redisCache.KeyDeleteAsync(uniqueKey);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public ValidationType GetSmsType(string type)
        {
            if (!Enum.TryParse(type, true, out ValidationType sendType))
            {
                sendType = ValidationType.UnKnow;
            }
            return sendType;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="type"></param>
        /// <param name="expiresMinute"></param>
        /// <returns></returns>
        public SmsContent GetSmsContent(ValidationType type, int expiresMinute = 5)
        {
       
            var code = new Random(Guid.NewGuid().GetHashCode()).Next(100000, 999999);

            if (_appSettings.DeveloperMode)
                code = 123456;

            string content;
            switch (type)
            {
                case ValidationType.Register:
                    content = $"您正在使用手机号注册福禄通行证账号，验证码：{code}，{expiresMinute}分钟内有效。如非本人操作，请忽略本短信。";
                    break;
                case ValidationType.ChangePhoneNo:
                    content = $"您正在修改手机号，验证码：{code}，{expiresMinute}分钟内有效。请勿将验证码泄露给他人使用。";
                    break;
                case ValidationType.ResetPassword:
                    content = $"您正在使用手机号进行修改密码操作，验证码：{code}，{expiresMinute}分钟内有效。验证码提供给让人可能导致账号被盗，请勿泄露，谨防被骗。";
                    break;
                case ValidationType.Login:
                    content = $"您正在使用短信验证码登录功能，验证码：{code}，{expiresMinute}分钟内有效。请勿将验证码泄露给他人使用。";
                    break;
                case ValidationType.Validate:
                    expiresMinute = 15;
                    content = $"您正在使用手机验证操作，验证码：{code}，{expiresMinute}分钟内有效。如非本人操作，请忽略本短信。";
                    break;
                default:
                    content = $"验证码：{code}，{expiresMinute}分钟内有效。请勿将验证码泄露给他人使用。";
                    break;
            }

            return new SmsContent
            {
                Code = code.ToString(),
                Content = content,
                ExpiresMinute = expiresMinute,
                ValidationType = type
            };
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="appId"></param>
        /// <param name="phone"></param>
        /// <param name="type"></param>
        /// <param name="ip"></param>
        /// <returns></returns>
        public async Task<DataContent<bool>> SendAsync(int appId, string phone, ValidationType type, string ip)
        {
            var smsModel = GetSmsContent(type);
            return await SendAsync(phone, smsModel.Code, appId, smsModel.Content, smsModel.ValidationType, ip, smsModel.ExpiresMinute);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="phone"></param>
        /// <param name="type"></param>
        /// <returns></returns>
        public async Task<bool> CheckOverLimit(string phone, ValidationType type)
        {
            var uniqueKey = GetSmsKey(phone, type);
            var smsLimit = await _redisCache.GetAsync<SmsCache>(uniqueKey);
            return smsLimit == null || DateTime.Now.AddSeconds(-30) >= smsLimit.StartTime;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="phone"></param>
        /// <param name="smsCode"></param>
        /// <param name="appId"></param>
        /// <param name="content"></param>
        /// <param name="type"></param>
        /// <param name="ip"></param>
        /// <param name="expiresMinute"></param>
        /// <returns></returns>
        public async Task<DataContent<bool>> SendAsync(string phone, string smsCode, int appId, string content, ValidationType type, string ip, int expiresMinute = 5)
        {
            var uniqueKey = GetSmsKey(phone, type);

            if (!await CheckOverLimit(phone, type))
                return Ok(false, "-1", "请求频率过高，请稍后再试");

            var smsLimit = new SmsCache { Code = smsCode, StartTime = DateTime.Now, ValidateCount = 0, ExpiresMinute = expiresMinute };

            await _redisCache.AddAsync(uniqueKey, smsLimit, new TimeSpan(0, 0, expiresMinute * 60));

            if (!_appSettings.DeveloperMode)
            {
                var (code, msg, msgId) = await _passportClient.SendSms(phone, content, _smsServerOptions.SignatureCode);

                _backgroundRunService.Transfer<IValidationComponent>(x =>
                    x.SaveLog(appId, content, type, phone, ip, code == "0", smsCode, expiresMinute, msgId, msg));
            }

            return Ok(true);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="phone"></param>
        /// <param name="type"></param>
        /// <returns></returns>
        public async Task<string> CreateTicketAsync(string phone, ValidationType type)
        {
            var ticket = Guid.NewGuid().ToString("N");
            var validationTicket = new ValidationTicket { Phone = phone, ValidationType = (int)type };
            await _redisCache.AddAsync(ticket, validationTicket, TimeSpan.FromMinutes(15));
            return ticket;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="ticket"></param>
        /// <returns></returns>
        public Task<ValidationTicket> GetValidateTicketAsync(string ticket)
        {
            return _redisCache.GetAsync<ValidationTicket>(ticket);
        }

    }
}
