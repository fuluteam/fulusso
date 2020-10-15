using System;
using System.Text;

namespace Fulu.BouncyCastle
{
public class HMACSHA256
{
    /// <summary>
    /// 生成签名
    /// </summary>
    public static byte[] GeneratorKey()
    {
        return HMAC.GeneratorKey(Algorithms.HMacSHA256);
    }

    /// <summary>
    /// 哈希计算（使用BouncyCastle）
    /// </summary>
    public static byte[] Compute(string data, byte[] key)
    {

        return HMAC.Compute(data, key, Algorithms.HMacSHA256);

        //or
        //return HMAC.Compute(data, key, new Sha256Digest());
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

        using (var hmacSha256 = new System.Security.Cryptography.HMACSHA256(key))
        {
            return hmacSha256.ComputeHash(Encoding.UTF8.GetBytes(data));
        }
    }
}
}
