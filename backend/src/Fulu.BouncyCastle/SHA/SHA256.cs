using System;
using System.Text;
using Org.BouncyCastle.Crypto.Digests;

namespace Fulu.BouncyCastle
{
    public class SHA256
    {
        /// <summary>
        /// 哈希计算（使用BouncyCastle）
        /// </summary>
        public static byte[] Compute1(string s)
        {
            if (string.IsNullOrEmpty(s))
            {
                throw new ArgumentNullException(nameof(s));
            }
            var digest = new Sha256Digest();
            var resBuf = new byte[digest.GetDigestSize()];
            var input = Encoding.UTF8.GetBytes(s);

            digest.BlockUpdate(input, 0, input.Length);
            digest.DoFinal(resBuf, 0);

            return resBuf;
        }

        /// <summary>
        /// 哈希计算（不使用BouncyCastle）
        /// </summary>
        public static byte[] Compute2(string s)
        {
            if (string.IsNullOrEmpty(s))
            {
                throw new ArgumentNullException(nameof(s));
            }

            using (var sha256 = System.Security.Cryptography.SHA256.Create())
            {
                return sha256.ComputeHash(Encoding.UTF8.GetBytes(s));
            }
        }
    }
}
