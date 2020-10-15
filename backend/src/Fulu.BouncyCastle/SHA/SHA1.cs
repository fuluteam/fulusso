using System;
using System.Text;
using Org.BouncyCastle.Crypto.Digests;

namespace Fulu.BouncyCastle
{
    public class SHA1
    {
        /// <summary>
        /// 哈希计算（使用BouncyCastle）
        /// </summary>
        public static byte[] Compute(string s)
        {
            if (string.IsNullOrEmpty(s))
            {
                throw new ArgumentNullException(nameof(s));
            }

            var digest = new Sha1Digest();
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

            using (var sha1 = System.Security.Cryptography.SHA1.Create())
            {
                return sha1.ComputeHash(Encoding.UTF8.GetBytes(s));
            }
        }
    }
}
