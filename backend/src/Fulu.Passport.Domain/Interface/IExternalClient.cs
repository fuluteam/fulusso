using Fulu.WebAPI.Abstractions;
using FuLu.Passport.Domain.Entities;
using FuLu.Passport.Domain.Models;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace FuLu.Passport.Domain.Interface
{
    /// <summary>
    /// 
    /// </summary>
    public interface IExternalClient
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="ip"></param>
        /// <returns></returns>
        Task<ResultBase<IpResult>> IpLocation(string ip);
        /// <summary>
        /// 
        /// </summary>
        /// <param name="ip"></param>
        /// <returns></returns>
        Task<string> IpToAddress(string ip);
        /// <summary>
        /// 使用百度提供的接口获取ip
        /// </summary>
        /// <param name="ip"></param>
        /// <returns></returns>
        Task<string> BdIpToAddress(string ip);
        /// <summary>
        /// 
        /// </summary>
        /// <param name="ticket"></param>
        /// <param name="randStr"></param>
        /// <returns></returns>
        Task<(int, string)> CaptchaTicketVerify(string ticket, string randStr);
    }
}
