using System;
using System.Security.Cryptography;
using System.Text;
using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Crypto.Parameters;
using Org.BouncyCastle.Security;
using Org.BouncyCastle.Utilities.Encoders;

namespace Fulu.BouncyCastle
{
    public class TripleDES
    {
        /// <summary>
        /// 密钥生成
        /// </summary>
        /// <param name="keySize">112位、128位、168位、192位</param>
        /// <returns></returns>
        public static byte[] GenerateKey(int keySize = 192)
        {
            var keyGen = GeneratorUtilities.GetKeyGenerator("DESEDE");
            keyGen.Init(new KeyGenerationParameters(new SecureRandom(), keySize));
            return keyGen.GenerateKey();
        }

        public static byte[] Encrypt(byte[] data, byte[] key, byte[] iv, CipherMode cipherMode, PaddingMode paddingMode)
        {
            if (data == null)
            {
                throw new ArgumentNullException(nameof(data));
            }

            if (key == null)
            {
                throw new ArgumentNullException(nameof(key));
            }

            if (iv == null)
            {
                throw new ArgumentNullException(nameof(iv));
            }

            using (var des = System.Security.Cryptography.TripleDES.Create())
            {
                des.Key = key;
                des.IV = iv;
                des.Mode = cipherMode;
                des.Padding = paddingMode;

                using (var ctf = des.CreateEncryptor())
                {
                    return ctf.TransformFinalBlock(data, 0, data.Length);
                }
            }
        }

        public static byte[] Decrypt(byte[] data, byte[] key, byte[] iv, CipherMode cipherMode, PaddingMode paddingMode)
        {
            if (data == null)
            {
                throw new ArgumentNullException(nameof(data));
            }

            if (key == null)
            {
                throw new ArgumentNullException(nameof(key));
            }

            if (iv == null)
            {
                throw new ArgumentNullException(nameof(iv));
            }

            using (var des = System.Security.Cryptography.TripleDES.Create())
            {
                des.Key = key;
                des.IV = iv;
                des.Mode = cipherMode;
                des.Padding = paddingMode;

                using (var ctf = des.CreateDecryptor())
                {
                    return ctf.TransformFinalBlock(data, 0, data.Length);
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
                cipher.Init(true, ParameterUtilities.CreateKeyParameter("DESEDE", key));
            }
            else
            {
                cipher.Init(true, new ParametersWithIV(ParameterUtilities.CreateKeyParameter("DESEDE", key), iv));
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
                cipher.Init(false, ParameterUtilities.CreateKeyParameter("DESEDE", key));
            }
            else
            {
                cipher.Init(false, new ParametersWithIV(ParameterUtilities.CreateKeyParameter("DESEDE", key), iv));
            }
            return cipher.DoFinal(data);
        }

        /// <summary>
        /// 加密数据并转Base64字符串
        /// </summary>
        /// <param name="data">待加密原文数据</param>
        /// <param name="key">密钥</param>
        /// <param name="iv">偏移量，ECB模式不用填写！</param>
        /// <param name="cipherMode">加密模式</param>
        /// <param name="paddingMode">填充模式</param>
        /// <returns>Base64字符串密文数据</returns>
        public static string EncryptToBase64(string data, string key, string iv, CipherMode cipherMode, PaddingMode paddingMode)
        {
            return Base64.ToBase64String(Encrypt(Encoding.UTF8.GetBytes(data), Encoding.UTF8.GetBytes(key), string.IsNullOrEmpty(iv) ? null : Encoding.UTF8.GetBytes(iv), cipherMode,
                 paddingMode));
        }
        /// <summary>
        /// 解密数据从Base64字符串
        /// </summary>
        /// <param name="data">待解密Base64字符串密文数据</param>
        /// <param name="key">密钥</param>
        /// <param name="iv">偏移量，ECB模式不用填写！</param>
        /// <param name="cipherMode">加密模式</param>
        /// <param name="paddingMode">填充模式</param>
        /// <returns>未加密原文数据</returns>
        public static string DecryptFromBase64(string data, string key, string iv, CipherMode cipherMode, PaddingMode paddingMode)
        {
            return Encoding.UTF8.GetString(Decrypt(Base64.Decode(data), Encoding.UTF8.GetBytes(key),
                string.IsNullOrEmpty(iv) ? null : Encoding.UTF8.GetBytes(iv), cipherMode, paddingMode));
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
        /// <summary>
        /// 加密数据并转Hex字符串
        /// </summary>
        /// <param name="data">待加密原文数据</param>
        /// <param name="key">密钥</param>
        /// <param name="iv">偏移量，ECB模式不用填写！</param>
        /// <param name="cipherMode">加密模式</param>
        /// <param name="paddingMode">填充模式</param>
        /// <returns>Hex字符串密文数据</returns>
        public static string EncryptToHex(string data, string key, string iv, CipherMode cipherMode, PaddingMode paddingMode)
        {
            return Hex.ToHexString(Encrypt(Encoding.UTF8.GetBytes(data), Encoding.UTF8.GetBytes(key), string.IsNullOrEmpty(iv) ? null : Encoding.UTF8.GetBytes(iv), cipherMode,
                paddingMode));
        }
        /// <summary>
        /// 解密数据从Hex字符串
        /// </summary>
        /// <param name="data">待解密Hex字符串密文数据</param>
        /// <param name="key">密钥</param>
        /// <param name="iv">偏移量，ECB模式不用填写！</param>
        /// <param name="cipherMode">加密模式</param>
        /// <param name="paddingMode">填充模式</param>
        /// <returns>未加密原文数据</returns>
        public static string DecryptFromHex(string data, string key, string iv, CipherMode cipherMode, PaddingMode paddingMode)
        {
            return Encoding.UTF8.GetString(Decrypt(Hex.Decode(data), Encoding.UTF8.GetBytes(key),
                string.IsNullOrEmpty(iv) ? null : Encoding.UTF8.GetBytes(iv), cipherMode, paddingMode));
        }
        /// <summary>
        /// 加密数据并转Hex字符串
        /// </summary>
        /// <param name="data">待加密原文数据</param>
        /// <param name="key">密钥</param>
        /// <param name="iv">偏移量，ECB模式不用填写！</param>
        /// <param name="algorithm">密文算法</param>
        /// <returns>Hex字符串密文数据</returns>
        public static string EncryptToHex(string data, string key, string iv, string algorithm)
        {
            return Hex.ToHexString(Encrypt(Encoding.UTF8.GetBytes(data), Encoding.UTF8.GetBytes(key), string.IsNullOrEmpty(iv) ? null : Encoding.UTF8.GetBytes(iv), algorithm));
        }
        /// <summary>
        /// 解密数据从Hex字符串
        /// </summary>
        /// <param name="data">待解密Hex字符串密文数据</param>
        /// <param name="key">密钥</param>
        /// <param name="iv">偏移量，ECB模式不用填写！</param>
        /// <param name="algorithm">密文算法</param>
        /// <returns>未加密原文数据</returns>
        public static string DecryptFromHex(string data, string key, string iv, string algorithm)
        {
            return Encoding.UTF8.GetString(Decrypt(Hex.Decode(data), Encoding.UTF8.GetBytes(key),
                string.IsNullOrEmpty(iv) ? null : Encoding.UTF8.GetBytes(iv), algorithm));
        }
    }
}
