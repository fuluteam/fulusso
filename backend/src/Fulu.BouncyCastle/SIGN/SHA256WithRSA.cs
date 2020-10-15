using System;
using System.Security.Cryptography;
using System.Text;
using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Security;
using Org.BouncyCastle.Utilities.Encoders;

namespace Fulu.BouncyCastle
{
    public class SHA256WithRSA
    {
        /// <summary>
        /// 生成签名
        /// </summary>
        public static string GenerateSignature(string data, RSAParameters privateKey)
        {
            if (string.IsNullOrEmpty(data))
            {
                throw new ArgumentNullException(nameof(data));
            }

            using (var rsa = System.Security.Cryptography.RSA.Create())
            {
                rsa.ImportParameters(privateKey);
                return Base64.ToBase64String(rsa.SignData(Encoding.UTF8.GetBytes(data), HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1));
            }
        }

        /// <summary>
        /// 验证签名
        /// </summary>
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
                return rsa.VerifyData(Encoding.UTF8.GetBytes(data), Base64.Decode(sign), HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1);
            }
        }

        /// <summary>
        /// 生成签名
        /// </summary>
        public static string GenerateSignature(string data, AsymmetricKeyParameter parameter)
        {
            if (string.IsNullOrEmpty(data))
            {
                throw new ArgumentNullException(nameof(data));
            }

            if (parameter == null)
            {
                throw new ArgumentNullException(nameof(parameter));
            }

            var signer = SignerUtilities.GetSigner("SHA256WithRSA");
            signer.Init(true, parameter);
            var bytes = Encoding.UTF8.GetBytes(data);
            signer.BlockUpdate(bytes, 0, bytes.Length);
            return Base64.ToBase64String(signer.GenerateSignature());
        }

        /// <summary>
        /// 验证签名
        /// </summary>
        public static bool VerifySignature(string data, string sign, AsymmetricKeyParameter parameter)
        {
            if (string.IsNullOrEmpty(data))
            {
                throw new ArgumentNullException(nameof(data));
            }

            if (string.IsNullOrEmpty(sign))
            {
                throw new ArgumentNullException(nameof(sign));
            }

            if (parameter == null)
            {
                throw new ArgumentNullException(nameof(parameter));
            }

            var verifier = SignerUtilities.GetSigner("SHA256WithRSA");
            verifier.Init(false, parameter);
            var bytes = Encoding.UTF8.GetBytes(data);
            verifier.BlockUpdate(bytes, 0, bytes.Length);
            return verifier.VerifySignature(Base64.Decode(sign));
        }
    }
}
