using System;
using System.Text;
using Org.BouncyCastle.Crypto.Digests;
using Org.BouncyCastle.Utilities.Encoders;

namespace Fulu.BouncyCastle
{
    public static class MD5
    {
        /// <summary>
        /// 哈希计算（使用BouncyCastle）
        /// </summary>
        /// <param name="s"></param>
        /// <returns></returns>
        public static string Compute(string s)
        {
            if (string.IsNullOrEmpty(s))
            {
                throw new ArgumentNullException(nameof(s));
            }
            var digest = new MD5Digest();
            var resBuf = new byte[digest.GetDigestSize()];
            var input = Encoding.UTF8.GetBytes(s);

            digest.BlockUpdate(input, 0, input.Length);
            digest.DoFinal(resBuf, 0);

            return Hex.ToHexString(resBuf).ToUpper();
        }

        /// <summary>
        /// 哈希计算（不使用BouncyCastle）
        /// </summary>
        /// <param name="s"></param>
        /// <returns></returns>
        public static string Compute2(string s)
        {
            if (string.IsNullOrEmpty(s))
            {
                throw new ArgumentNullException(nameof(s));
            }

            using (var md5 = System.Security.Cryptography.MD5.Create())
            {
                return Hex.ToHexString(md5.ComputeHash(Encoding.UTF8.GetBytes(s)));
            }
        }
    }
}
