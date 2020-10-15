using System;
using System.Text;

namespace Fulu.BouncyCastle
{
public class HMACMD5
{
    /// <summary>
    /// 生成密钥KEY
    /// </summary>
    /// <returns></returns>
    public static byte[] GeneratorKey()
    {
        return HMAC.GeneratorKey(Algorithms.HMacMD5);
    }

    /// <summary>
    /// 哈希计算（使用BouncyCastle）
    /// </summary>
    public static byte[] Compute(string data, byte[] key)
    {
        return HMAC.Compute(data, key, Algorithms.HMacMD5);
        //or
        //return HMAC.Compute(data, key, new MD5Digest());
    }

    /// <summary>
    /// 哈希计算（不使用BouncyCastle）
    /// </summary>
    public static byte[] Compute2(string data, string key)
    {
        if (string.IsNullOrEmpty(data))
        {
            throw new ArgumentNullException(nameof(data));
        }

        if (string.IsNullOrEmpty(key))
        {
            throw new ArgumentNullException(nameof(key));
        }

        using (var hmacMd5 = new System.Security.Cryptography.HMACMD5(Encoding.UTF8.GetBytes(key)))
        {
            return hmacMd5.ComputeHash(Encoding.UTF8.GetBytes(data));
        }
    }
}
}
