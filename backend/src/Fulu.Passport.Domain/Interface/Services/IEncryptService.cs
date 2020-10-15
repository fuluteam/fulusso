using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Fulu.AutoDI;

namespace Fulu.Passport.Domain.Interface.Services
{
    public interface IEncryptService : ISingletonAutoDIable
    {
        /// <summary>
        /// 加密明文密码（入库存储）
        /// </summary>
        /// <returns></returns>
        string EncryptAes(string password);
        /// <summary>
        /// 解密前端传入密码
        /// </summary>
        /// <returns></returns>
        string DecryptRsa(string cipherText);
    }
}
