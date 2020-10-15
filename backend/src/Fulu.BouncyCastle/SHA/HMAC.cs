using System;
using System.Text;
using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Security;

namespace Fulu.BouncyCastle
{
    public class HMAC
    {
        /// <summary>
        /// 生成密钥KEY
        /// </summary>
        /// <param name="algorithm">密文算法，参考Algorithms.cs中提供的HMac algorithm</param>
        /// <returns>密钥KEY</returns>
        public static byte[] GeneratorKey(string algorithm)
        {
            var kGen = GeneratorUtilities.GetKeyGenerator(algorithm);
            return kGen.GenerateKey();
        }

        /// <summary>
        /// 哈希计算
        /// </summary>
        /// <param name="data">输入字符串</param>
        /// <param name="key">密钥KEY</param>
        /// <param name="algorithm">密文算法，参考Algorithms.cs中提供的HMac algorithm</param>
        /// <returns>哈希值</returns>
        public static byte[] Compute(string data, byte[] key, string algorithm)
        {
            if (string.IsNullOrEmpty(data))
            {
                throw new ArgumentNullException(nameof(data));
            }

            var keyParameter = new Org.BouncyCastle.Crypto.Parameters.KeyParameter(key);
            var input = Encoding.UTF8.GetBytes(data);
            var mac = MacUtilities.GetMac(algorithm);
            mac.Init(keyParameter);
            mac.BlockUpdate(input, 0, input.Length);
            return MacUtilities.DoFinal(mac);
        }

        /// <summary>
        /// 哈希计算
        /// </summary>
        /// <param name="data">输入字符串</param>
        /// <param name="key">密钥KEY</param>
        /// <param name="digest"></param>
        /// <returns>哈希值</returns>
        public static byte[] Compute(string data, byte[] key, IDigest digest)
        {
            if (string.IsNullOrEmpty(data))
            {
                throw new ArgumentNullException(nameof(data));
            }

            var keyParameter = new Org.BouncyCastle.Crypto.Parameters.KeyParameter(key);
            var input = Encoding.UTF8.GetBytes(data);
            IMac mac = new Org.BouncyCastle.Crypto.Macs.HMac(digest);
            mac.Init(keyParameter);
            mac.BlockUpdate(input, 0, input.Length);
            return MacUtilities.DoFinal(mac);
        }
    }
}
