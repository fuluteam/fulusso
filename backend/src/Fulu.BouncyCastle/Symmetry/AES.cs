using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Crypto.Parameters;
using Org.BouncyCastle.Security;
using System;
using System.Collections.Generic;
using System.Text;
using Org.BouncyCastle.Utilities;
using System.Security.Cryptography;
using System.Linq;
using Org.BouncyCastle.Utilities.Encoders;

namespace Fulu.BouncyCastle
{
    public class AES
    {
        /// <summary>
        /// 生成KEY
        /// </summary>
        /// <param name="keySize">128位、192位、256位</param>
        /// <returns></returns>
        public static byte[] GenerateKey(int keySize = 128)
        {
            var kg = GeneratorUtilities.GetKeyGenerator("AES");
            kg.Init(new KeyGenerationParameters(new SecureRandom(), keySize));
            return kg.GenerateKey();
        }

        /// <summary>
        /// 网上流传一种利润SecureRandom输出固定随机值并用这个随机值当做加密密钥的用法
        /// 这种方式确实可以对原始密钥做一定的隐藏。起到混淆作用。但Google官方否定了该方式的使用。
        /// </summary>
        public static byte[] GetKey(string s)
        {
            using (var st = new SHA1CryptoServiceProvider())
            {
                using (var sha1Crypto = new SHA1CryptoServiceProvider())
                {
                    return sha1Crypto.ComputeHash(st.ComputeHash(Strings.ToUtf8ByteArray(s))).Take(16).ToArray();
                }
            }
        }

        /// <summary>
        /// 加密
        /// </summary>
        /// <param name="data">待加密原文数据</param>
        /// <param name="key">密钥</param>
        /// <param name="iv">偏移量，ECB模式不用填写！</param>
        /// <param name="algorithm">密文算法</param>
        /// <returns>密文数据</returns>
        public static byte[] Encrypt(byte[] data, byte[] key, byte[] iv, string algorithm)
        {
            if (data == null)
            {
                throw new ArgumentNullException(nameof(data));
            }

            if (key == null)
            {
                throw new ArgumentNullException(nameof(key));
            }

            var cipher = CipherUtilities.GetCipher(algorithm);
            if (iv == null)
            {
                cipher.Init(true, ParameterUtilities.CreateKeyParameter("AES", key));
            }
            else
            {
                cipher.Init(true, new ParametersWithIV(ParameterUtilities.CreateKeyParameter("AES", key), iv));
            }

            return cipher.DoFinal(data);
        }
        /// <summary>
        /// 解密
        /// </summary>
        /// <param name="data">待解密数据</param>
        /// <param name="key">密钥</param>
        /// <param name="iv">偏移量，ECB模式不用填写！</param>
        /// <param name="algorithm">密文算法</param>
        /// <returns>未加密原文数据</returns>
        public static byte[] Decrypt(byte[] data, byte[] key, byte[] iv, string algorithm)
        {
            if (data == null)
            {
                throw new ArgumentNullException(nameof(data));
            }

            if (key == null)
            {
                throw new ArgumentNullException(nameof(key));
            }

            var cipher = CipherUtilities.GetCipher(algorithm);
            if (iv == null)
            {
                cipher.Init(false, ParameterUtilities.CreateKeyParameter("AES", key));
            }
            else
            {
                cipher.Init(false, new ParametersWithIV(ParameterUtilities.CreateKeyParameter("AES", key), iv));
            }
            return cipher.DoFinal(data);
        }

        /// <summary>
        /// 加密数据并转Base64字符串
        /// </summary>
        /// <param name="data">待加密原文数据</param>
        /// <param name="key">密钥</param>
        /// <param name="iv">偏移量，ECB模式不用填写！</param>
        /// <param name="algorithm">密文算法</param>
        /// <returns>Base64字符串密文数据</returns>
        public static string EncryptToBase64(string data, string key, string iv, string algorithm)
        {
            return Base64.ToBase64String(Encrypt(Encoding.UTF8.GetBytes(data), Encoding.UTF8.GetBytes(key), string.IsNullOrEmpty(iv) ? null : Encoding.UTF8.GetBytes(iv), algorithm));
        }
        /// <summary>
        /// 解密数据从Base64字符串
        /// </summary>
        /// <param name="data">待解密Base64字符串密文数据</param>
        /// <param name="key">密钥</param>
        /// <param name="iv">偏移量，ECB模式不用填写！</param>
        /// <param name="algorithm">密文算法</param>
        /// <returns>未加密原文数据</returns>
        public static string DecryptFromBase64(string data, string key, string iv, string algorithm)
        {
            return Encoding.UTF8.GetString(Decrypt(Base64.Decode(data), Encoding.UTF8.GetBytes(key),
                string.IsNullOrEmpty(iv) ? null : Encoding.UTF8.GetBytes(iv), algorithm));
        }
    }
}
