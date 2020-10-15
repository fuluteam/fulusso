using Fulu.AutoDI;
using Fulu.Passport.Domain.Component;
using Fulu.Passport.Domain.Models;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Fulu.WebAPI.Abstractions;

namespace Fulu.Passport.Domain.Interface
{
    /// <summary>
    /// 
    /// </summary>
    public interface IValidationComponent : IScopedAutoDIable
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="phone"></param>
        /// <param name="type"></param>
        /// <returns></returns>
        Task<bool> CheckOverLimit(string phone, ValidationType type);
        /// <summary>
        /// 
        /// </summary>
        Task<DataContent<bool>> ValidSmsAsync(string phone, string code, ValidationType type);

        /// <summary>
        /// 
        /// </summary>
        Task ClearSession(string phone, ValidationType type);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        ValidationType GetSmsType(string type);

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
        Task SaveLog(int appId, string content, ValidationType type, string phone, string ip,
            bool isSuccess, string code, int expiresMinute, string smsMsgId, string smsMsg);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="type"></param>
        /// <param name="expiresMinute"></param>
        /// <returns></returns>
        SmsContent GetSmsContent(ValidationType type, int expiresMinute = 5);
        /// <summary>
        /// 
        /// </summary>
        /// <param name="appId"></param>
        /// <param name="phone"></param>
        /// <param name="type"></param>
        /// <param name="ip"></param>
        /// <returns></returns>
        Task<DataContent<bool>> SendAsync(int appId, string phone, ValidationType type, string ip);

        /// <summary>
        /// 发送内容信息
        /// </summary>
        Task<DataContent<bool>> SendAsync(string phone, string code, int appId, string content, ValidationType type, string ip, int expiresMinute = 5);
        /// <summary>
        /// 
        /// </summary>
        /// <param name="phone"></param>
        /// <param name="type"></param>
        /// <returns></returns>
        Task<string> CreateTicketAsync(string phone, ValidationType type);
        /// <summary>
        /// 
        /// </summary>
        /// <param name="ticket"></param>
        /// <returns></returns>
        Task<ValidationTicket> GetValidateTicketAsync(string ticket);

    }
}
