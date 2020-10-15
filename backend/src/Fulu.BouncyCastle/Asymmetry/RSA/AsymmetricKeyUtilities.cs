using System;
using System.Security.Cryptography;
using Org.BouncyCastle.Asn1;
using Org.BouncyCastle.Asn1.Pkcs;
using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Crypto.Parameters;
using Org.BouncyCastle.Security;
using Org.BouncyCastle.Utilities.Encoders;

namespace Fulu.BouncyCastle
{
    /// <summary>
    /// 
    /// </summary>
    public class AsymmetricKeyUtilities
    {
        /// <summary>
        /// -----BEGIN RSA PRIVATE KEY-----
        /// ...
        /// -----END RSA PRIVATE KEY-----
        /// </summary>
        /// <param name="privateKey">Pkcs1格式私钥</param>
        /// <returns></returns>
        public static AsymmetricKeyParameter GetAsymmetricKeyParameterFormPrivateKey(string privateKey)
        {
            if (string.IsNullOrEmpty(privateKey))
            {
                throw new ArgumentNullException(nameof(privateKey));
            }

            var instance = RsaPrivateKeyStructure.GetInstance(Base64.Decode(privateKey));
            return new RsaPrivateCrtKeyParameters(instance.Modulus, instance.PublicExponent, instance.PrivateExponent, instance.Prime1, instance.Prime2, instance.Exponent1, instance.Exponent2, instance.Coefficient);
        }

        /// <summary>
        /// -----BEGIN PRIVATE KEY-----
        /// ...
        /// -----END PRIVATE KEY-----
        /// </summary>
        /// <param name="privateKey">Pkcs8格式私钥</param>
        /// <returns></returns>
        public static AsymmetricKeyParameter GetAsymmetricKeyParameterFormAsn1PrivateKey(string privateKey)
        {
            return PrivateKeyFactory.CreateKey(Base64.Decode(privateKey));
        }

        /// <summary>
        /// PUBLIC KEY
        /// </summary>
        /// <param name="publicKey">公钥</param>
        /// <returns></returns>
        public static AsymmetricKeyParameter GetAsymmetricKeyParameterFormPublicKey(string publicKey)
        {
            if (string.IsNullOrEmpty(publicKey))
            {
                throw new ArgumentNullException(nameof(publicKey));
            }

            return PublicKeyFactory.CreateKey(Base64.Decode(publicKey));
        }

        /// <summary>
        /// -----BEGIN RSA PRIVATE KEY-----
        /// ...
        /// -----END RSA PRIVATE KEY-----
        /// </summary>
        /// <param name="privateKey">Pkcs1格式私钥</param>
        /// <returns></returns>
        public static RSAParameters GetRsaParametersFormPrivateKey(string privateKey)
        {
            if (string.IsNullOrEmpty(privateKey))
            {
                throw new ArgumentNullException(nameof(privateKey));
            }

            var instance = RsaPrivateKeyStructure.GetInstance(Base64.Decode(privateKey));
            return new RSAParameters
            {
                D = instance.PrivateExponent.ToByteArrayUnsigned(),
                DP = instance.Exponent1.ToByteArrayUnsigned(),
                DQ = instance.Exponent2.ToByteArrayUnsigned(),
                Exponent = instance.PublicExponent.ToByteArrayUnsigned(),
                InverseQ = instance.Coefficient.ToByteArrayUnsigned(),
                Modulus = instance.Modulus.ToByteArrayUnsigned(),
                P = instance.Prime1.ToByteArrayUnsigned(),
                Q = instance.Prime2.ToByteArrayUnsigned(),
            };
        }

        /// <summary>
        /// -----BEGIN PRIVATE KEY-----
        /// ...
        /// -----END PRIVATE KEY-----
        /// </summary>
        /// <param name="privateKey">Pkcs8格式私钥</param>
        /// <returns></returns>
        public static RSAParameters GetRsaParametersFormAsn1PrivateKey(string privateKey)
        {
            var keyInfo = PrivateKeyInfo.GetInstance(Asn1Object.FromByteArray(Base64.Decode(privateKey)));
            
            var instance = RsaPrivateKeyStructure.GetInstance(keyInfo.ParsePrivateKey());

            return new RSAParameters
            {
                D = instance.PrivateExponent.ToByteArrayUnsigned(),
                DP = instance.Exponent1.ToByteArrayUnsigned(),
                DQ = instance.Exponent2.ToByteArrayUnsigned(),
                Exponent = instance.PublicExponent.ToByteArrayUnsigned(),
                InverseQ = instance.Coefficient.ToByteArrayUnsigned(),
                Modulus = instance.Modulus.ToByteArrayUnsigned(),
                P = instance.Prime1.ToByteArrayUnsigned(),
                Q = instance.Prime2.ToByteArrayUnsigned(),
            };
        }

        /// <summary>
        /// PUBLIC KEY
        /// </summary>
        /// <param name="publicKey">公钥</param>
        /// <returns></returns>
        public static RSAParameters GetRsaParametersFormPublicKey(string publicKey)
        {
            if (string.IsNullOrEmpty(publicKey))
            {
                throw new ArgumentNullException(nameof(publicKey));
            }

            var keyParameters = (RsaKeyParameters)PublicKeyFactory.CreateKey(Base64.Decode(publicKey));
            return new RSAParameters
            {
                Modulus = keyParameters.Modulus.ToByteArrayUnsigned(),
                Exponent = keyParameters.Exponent.ToByteArrayUnsigned(),
            };
        }
    }
}