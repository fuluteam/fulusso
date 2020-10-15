using Fulu.BouncyCastle;
using Fulu.Passport.Domain.Interface.Services;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Org.BouncyCastle.Utilities;
using Org.BouncyCastle.Utilities.Encoders;
using FuLu.Passport.Domain.Options;

namespace Fulu.Passport.Domain.Services
{
    public class EncryptService : IEncryptService
    {
        private readonly ILogger<IEncryptService> _logger;
        private readonly AppSettings _appSettings;
        public EncryptService(AppSettings appSettings, ILogger<IEncryptService> logger)
        {
            _appSettings = appSettings;
            _logger = logger;
        }
        /// <summary>
        /// 加密明文密码入库
        /// </summary>
        public string EncryptAes(string password)
        {
            return AES.EncryptToBase64(password, _appSettings.AesKey, _appSettings.AesIv, Algorithms.AES_CBC_PKCS7Padding);
        }

        /// <summary>
        /// 解密输入前端密码
        /// </summary>
        /// <param name="cipherText"></param>
        /// <returns></returns>
        public  string DecryptRsa(string cipherText)
        {
            try
            {
                Console.WriteLine(cipherText);
                Console.WriteLine(_appSettings.PrivateKey);
                var password = RSA.DecryptFromHex(cipherText, AsymmetricKeyUtilities.GetAsymmetricKeyParameterFormPrivateKey(_appSettings.PrivateKey),
                    Algorithms.RSA_NONE_PKCS1Padding);

                password = Strings.FromByteArray(Base64.Decode(password));

                return password;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.ToString());
                return string.Empty;
            }
        }
    }
}
