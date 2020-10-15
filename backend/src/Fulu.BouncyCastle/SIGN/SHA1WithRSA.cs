using System;
using System.Security.Cryptography;
using System.Text;
using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Security;
using Org.BouncyCastle.Utilities.Encoders;

namespace Fulu.BouncyCastle
{
public class SHA1WithRSA
{
    /// <summary>
    /// 生成签名
    /// </summary>
    /// <param name="data"></param>
    /// <param name="privateKey"></param>
    /// <returns></returns>
    public static string GenerateSignature(string data, RSAParameters privateKey)
    {
        if (string.IsNullOrEmpty(data))
        {
            throw new ArgumentNullException(nameof(data));
        }

        using (var rsa = System.Security.Cryptography.RSA.Create())
        {
            rsa.ImportParameters(privateKey);
            return Base64.ToBase64String(rsa.SignData(Encoding.UTF8.GetBytes(data), HashAlgorithmName.SHA1, RSASignaturePadding.Pkcs1));
        }
    }

    /// <summary>
    /// 验证签名
    /// </summary>
    /// <param name="data"></param>
    /// <param name="sign"></param>
    /// <param name="publicKey"></param>
    /// <returns></returns>
    public static bool VerifySignature(string data, string sign, RSAParameters publicKey)
    {
        if (string.IsNullOrEmpty(data))
        {
            throw new ArgumentNullException(nameof(data));
        }

        if (string.IsNullOrEmpty(sign))
        {
            throw new ArgumentNullException(nameof(sign));
        }

        using (var rsa = System.Security.Cryptography.RSA.Create())
        {
            rsa.ImportParameters(publicKey);
            return rsa.VerifyData(Encoding.UTF8.GetBytes(data), Base64.Decode(sign), HashAlgorithmName.SHA1, RSASignaturePadding.Pkcs1);
        }
    }

    /// <summary>
    /// 生成签名
    /// </summary>
    public static string GenerateSignature(string data, AsymmetricKeyParameter privateKey)
    {
        var byteData = Encoding.UTF8.GetBytes(data);
        var normalSig = SignerUtilities.GetSigner("SHA1WithRSA");
        normalSig.Init(true, privateKey);
        normalSig.BlockUpdate(byteData, 0, data.Length);
        var normalResult = normalSig.GenerateSignature();
        return Base64.ToBase64String(normalResult);
    }

    /// <summary>
    /// 验证签名
    /// </summary>
    /// <param name="data"></param>
    /// <param name="sign"></param>
    /// <param name="publicKey"></param>
    /// <returns></returns>
    public static bool VerifySignature(string data, string sign, AsymmetricKeyParameter publicKey)
    {
        var signBytes = Base64.Decode(sign);
        var plainBytes = Encoding.UTF8.GetBytes(data);
        var verifier = SignerUtilities.GetSigner("SHA1WithRSA");
        verifier.Init(false, publicKey);
        verifier.BlockUpdate(plainBytes, 0, plainBytes.Length);

        return verifier.VerifySignature(signBytes);
    }
}
}
