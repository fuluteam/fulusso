using System;
using System.Text;

namespace Fulu.BouncyCastle
{
    public class HMACSHA1
    {
        /// <summary>
        /// 生成密钥KEY
        /// </summary>
        /// <returns></returns>
        public static byte[] GeneratorKey()
        {
            return HMAC.GeneratorKey(Algorithms.HMacSHA1);
        }

        /// <summary>
        /// 哈希计算（使用BouncyCastle）
        /// </summary>
        public static byte[] Compute(string data, byte[] key)
        {
            return HMAC.Compute(data, key, Algorithms.HMacSHA1);
            //or
            //return HMAC.Compute(data, key, new Sha1Digest());
        }

        /// <summary>
        /// 哈希计算（不使用BouncyCastle）
        /// </summary>
        public static byte[] Compute2(string data, byte[] key)
        {
            if (string.IsNullOrEmpty(data))
            {
                throw new ArgumentNullException(nameof(data));
            }

            using (var hmacSha1 = new System.Security.Cryptography.HMACSHA1(key))
            {
                return hmacSha1.ComputeHash(Encoding.UTF8.GetBytes(data));
            }
        }
    }
}
