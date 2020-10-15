using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using System.Security.Claims;
using Fulu.AutoDI;
using FuLu.Passport.Domain.Entities;
using FuLu.Passport.Domain.Models;
using Fulu.WebAPI.Abstractions;

namespace FuLu.Passport.Domain.Interface.Services
{
    /// <summary>
    /// 
    /// </summary>
    public interface IUserService : IScopedAutoDIable
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="phone"></param>
        /// <returns></returns>
        Task<UserEntity> GetUserByPhoneAsync(string phone);

        /// <summary>
        /// 手机号是否存在
        /// </summary>
        Task<bool> ExistPhoneAsync(string phone);
        /// <summary>
        /// 
        /// </summary>
        /// <param name="phone"></param>
        /// <param name="password"></param>
        /// <param name="clientId"></param>
        /// <param name="clientName"></param>
        /// <param name="ip"></param>
        /// <param name="nickName"></param>
        /// <returns></returns>
        Task<UserEntity> RegisterAsync(string phone, string password, int clientId, string clientName, string ip,
            string nickName = "");
        /// <summary>
        /// 
        /// </summary>
        /// <param name="phone"></param>
        /// <param name="newPassword"></param>
        /// <returns></returns>
        Task ResetPasswordAsync(string phone, string newPassword);

        /// <summary>
        /// 更换手机号
        /// </summary>
        /// <returns></returns>
        Task<(string msg, string code)> ChangePhoneAsync(string phone, string newPhone);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="userId"></param>
        /// <returns></returns>
        Task SaveErrorLoginInfo(string userId);
        /// <summary>
        /// 
        /// </summary>
        /// <param name="userEntity"></param>
        /// <param name="clientId"></param>
        /// <param name="loginIp"></param>
        /// <param name="loginModel"></param>
        /// <returns></returns>
        Task<IEnumerable<Claim>> GetLoginClaims(UserEntity userEntity, int clientId, string loginIp,
            UserLoginModel loginModel);

        Task<DataContent<UserEntity>> LoginByPasswordAsync(string phone, string password);
    }
}
