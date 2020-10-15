using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using Fulu.BouncyCastle;
using Fulu.Passport.Domain.Interface;
using Fulu.Passport.Domain.Interface.Services;
using Fulu.WebAPI.Abstractions;
using IdentityModel;
using Microsoft.Extensions.Caching.Redis;
using Microsoft.Extensions.Logging;
using Microsoft.EntityFrameworkCore;
using FuLu.Passport.Domain.Interface.Services;
using FuLu.Passport.Domain.Interface.Repositories;
using FuLu.Passport.Domain.Entities;
using FuLu.Passport.Domain.Interface;
using FuLu.Passport.Domain.Models;
using FuLu.Passport.Domain.Options;

namespace FuLu.Passport.Domain.Services
{
    /// <summary>
    /// 用户
    /// </summary>
    public class UserService : ResultBase, IUserService
    {
        private readonly IRedisCache _redisCache;
        private readonly IUserRepository _userRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IEncryptService _encryptService;
        private readonly IExternalClient _externalClient;
        /// <summary>
        /// 
        /// </summary>
        /// <param name="redisCache"></param>
        /// <param name="userRepository"></param>
        /// <param name="unitOfWork"></param>
        /// <param name="encryptService"></param>
        /// <param name="externalClient"></param>
        public UserService(IRedisCache redisCache, IUserRepository userRepository, IUnitOfWork unitOfWork, IEncryptService encryptService, IExternalClient externalClient)
        {
            _redisCache = redisCache;
            _userRepository = userRepository;
            _unitOfWork = unitOfWork;
            _encryptService = encryptService;
            _externalClient = externalClient;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="phone"></param>
        /// <returns></returns>
        public async Task<UserEntity> GetUserByPhoneAsync(string phone)
        {
            var userEntity = new UserEntity { Phone = phone };

            userEntity = await _redisCache.ObjectGetAsync(userEntity);
            if (userEntity != null)
            {
                return userEntity;
            }
            userEntity = await _userRepository.TableNoTracking.FirstOrDefaultAsync(c => c.Phone == phone);
            if (userEntity != null)
            {
                await _redisCache.ObjectAddAsync(userEntity, TimeSpan.FromDays(15));
            }
            return userEntity;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="phone"></param>
        /// <returns></returns>
        public async Task<bool> ExistPhoneAsync(string phone)
        {
            var userEntity = await GetUserByPhoneAsync(phone);
            return userEntity != null;
        }
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
        public async Task<UserEntity> RegisterAsync(string phone, string password, int clientId, string clientName, string ip, string nickName = "")
        {
            var passwd = _encryptService.EncryptAes(password);
            var userEntity = new UserEntity
            {
                Id = Guid.NewGuid().ToString("D"),
                NickName = nickName,
                LoginCount = 0,
                LoginErrorCount = 0,
                Phone = phone,
                UserName = Guid.NewGuid().ToString("D"),
                Gender = 0,
                Password = passwd,
                Birthday = DateTime.Now,
                RegisterTime = DateTime.Now,
                RegisterIp = ip,
                RegisterClientId = clientId,
                RegisterClientName = clientName,
                Enabled = true,
                LastLoginTime = DateTime.Now,
                LastLoginIp = ip,
                LastTryLoginTime = null,
                EnabledTwoFactor = true
            };

            await _userRepository.InsertAsync(userEntity);
            await _unitOfWork.SaveChangesAsync();

            await _redisCache.ObjectAddAsync(userEntity);
            return userEntity;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="phone"></param>
        /// <param name="newPassword"></param>
        /// <returns></returns>
        public async Task ResetPasswordAsync(string phone, string newPassword)
        {
            var userEntity = await _userRepository.Table.FirstOrDefaultAsync(c => c.Phone == phone);
            userEntity.Password = _encryptService.EncryptAes(newPassword);
            userEntity.LoginErrorCount = 0;
            userEntity.LastTryLoginTime = null;
            userEntity.Locked = false;

            await _unitOfWork.SaveChangesAsync();

            await _redisCache.ObjectAddAsync(userEntity);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="phone"></param>
        /// <param name="newPhone"></param>
        /// <returns></returns>
        public Task<(string msg, string code)> ChangePhoneAsync(string phone, string newPhone)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="userId"></param>
        /// <returns></returns>
        public async Task SaveErrorLoginInfo(string userId)
        {
            var userEntity = await _userRepository.Table.FirstOrDefaultAsync(x => x.Id == userId);
            if (userEntity.LastTryLoginTime.HasValue &&
               userEntity.LastTryLoginTime.Value.AddHours(3) <= DateTime.Now)
            {//锁定超时重置锁定信息
                userEntity.Locked = false;
                userEntity.LoginErrorCount = 0;
                userEntity.LastTryLoginTime = null;
            }

            userEntity.LoginErrorCount = userEntity.LoginErrorCount + 1;
            userEntity.LastTryLoginTime = DateTime.Now;
            if (userEntity.LoginErrorCount >= 5)
            {//登录密码连续输错5次
                //登录密码已被锁定，请3小时候再试，建议您找回密码。
                userEntity.Locked = true;
            }
            await _unitOfWork.SaveChangesAsync();
            await _redisCache.ObjectAddAsync(userEntity);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="userEntity"></param>
        /// <param name="clientId"></param>
        /// <param name="loginIp"></param>
        /// <param name="loginModel"></param>
        /// <returns></returns>
        public async Task<IEnumerable<Claim>> GetLoginClaims(UserEntity userEntity, int clientId, string loginIp, UserLoginModel loginModel)
        {
            var loginAddress = string.Empty;
            if (!string.IsNullOrEmpty(loginIp))
            {
                loginAddress = await _externalClient.BdIpToAddress(loginIp);
            }

            var openId = MD5.Compute($"openId{clientId}{userEntity.Id}");
            var claims = new[]
            {
                new Claim(JwtClaimTypes.Subject, userEntity.Id),
                new Claim(JwtClaimTypes.Id, userEntity.Id),
                new Claim(CusClaimTypes.OpenId, openId),
                new Claim(JwtClaimTypes.ClientId, clientId.ToString()), 
                new Claim(JwtClaimTypes.Name, userEntity.UserName),
                new Claim(JwtClaimTypes.NickName, userEntity.NickName ?? ""),
                new Claim(JwtClaimTypes.PhoneNumber, userEntity.Phone ?? ""),
                new Claim(JwtClaimTypes.Email, userEntity.Email ?? ""),
                new Claim(JwtClaimTypes.Role, "User"),
                new Claim(ClaimTypes.Role, "User"),
                new Claim(CusClaimTypes.LoginIp, loginIp??""),
                new Claim(JwtClaimTypes.AuthenticationMethod, "mfa"),
                new Claim(CusClaimTypes.LoginAddress, loginAddress??""),
                new Claim(CusClaimTypes.LastLoginIp, userEntity.LastLoginIp??""),
                new Claim(CusClaimTypes.LastLoginAddress, userEntity.LastLoginAddress??""),
            };
            return claims;
        }


        public async Task<DataContent<UserEntity>> LoginByPasswordAsync(string phone, string cipherPass)
        {
            var password = _encryptService.DecryptRsa(cipherPass);
            if (string.IsNullOrEmpty(password))
                return Ok(default(UserEntity), "-1", "密码格式不正确");

            var userEntity = await GetUserByPhoneAsync(phone);
            if (userEntity == null)
                return Ok(default(UserEntity), "-1", "用户名或密码不正确");

            if (userEntity.Enabled == false)
                return Ok(default(UserEntity), "-1", "您的账号已被禁用");

            var lastTypeLoginTime = userEntity.LastTryLoginTime ?? DateTime.Now;

            if (userEntity.Locked && lastTypeLoginTime.AddHours(3) > DateTime.Now)
                return Ok(default(UserEntity), "-1", "登录密码出错已达上限将锁定密码3小时，请找回密码后登录，或使用短信登录。");

            if (_encryptService.EncryptAes(password) != userEntity.Password)
            {
                await SaveErrorLoginInfo(userEntity.Id);
                if (userEntity.LoginErrorCount < 2)
                {
                    return Ok(default(UserEntity), "-1", "用户名或密码不正确");
                }

                var count = 5 - userEntity.LoginErrorCount;
                count = count < 0 ? 0 : count;
                var msg = $"用户名或密码不正确，还有{count}次机会。您还可以：重置登录密码";
                if (count <= 0)
                {
                    msg = "登录密码出错已达上限将锁定密码3小时，请找回密码后登录，或使用短信登录。";
                }
                return Ok(default(UserEntity), "-1", msg);
            }

            return Ok(userEntity);
        }

    }
}
