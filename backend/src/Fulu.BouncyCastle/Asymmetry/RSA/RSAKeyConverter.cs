using System.IO;
using Org.BouncyCastle.Asn1;
using Org.BouncyCastle.Asn1.Pkcs;
using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Crypto.Parameters;
using Org.BouncyCastle.OpenSsl;
using Org.BouncyCastle.Pkcs;
using Org.BouncyCastle.Utilities.Encoders;
using Org.BouncyCastle.X509;

namespace Fulu.BouncyCastle
{
    public class RSAKeyConverter
    {
        /// <summary>
        /// Pkcs1>>Pkcs8
        /// </summary>
        /// <param name="privateKey">Pkcs1私钥</param>
        /// <param name="format">是否转PEM格式</param>
        /// <returns></returns>
        public static string PrivateKeyPkcs1ToPkcs8(string privateKey, bool format = false)
        {
            var akp = AsymmetricKeyUtilities.GetAsymmetricKeyParameterFormPrivateKey(privateKey);
            if (format)
            {
                var sw = new StringWriter();
                var pWrt = new PemWriter(sw);
                var pkcs8 = new Pkcs8Generator(akp);
                pWrt.WriteObject(pkcs8);
                pWrt.Writer.Close();
                return sw.ToString();
            }
            else
            {
                var privateKeyInfo = PrivateKeyInfoFactory.CreatePrivateKeyInfo(akp);
                return Base64.ToBase64String(privateKeyInfo.GetEncoded());
            }
        }

        /// <summary>
        /// Pkcs8>>Pkcs1
        /// </summary>
        /// <param name="privateKey">Pkcs8私钥</param>
        /// <param name="format">是否转PEM格式</param>
        /// <returns></returns>
        public static string PrivateKeyPkcs8ToPkcs1(string privateKey, bool format = false)
        {
            var akp = AsymmetricKeyUtilities.GetAsymmetricKeyParameterFormAsn1PrivateKey(privateKey);
            if (format)
            {
                var sw = new StringWriter();
                var pWrt = new PemWriter(sw);
                pWrt.WriteObject(akp);
                pWrt.Writer.Close();
                return sw.ToString();
            }
            else
            {
                var privateKeyInfo = PrivateKeyInfoFactory.CreatePrivateKeyInfo(akp);
                return Base64.ToBase64String(privateKeyInfo.ParsePrivateKey().GetEncoded());
            }
        }

        /// <summary>
        /// 从Pkcs8私钥中提取公钥
        /// </summary>
        /// <param name="privateKey">Pkcs8私钥</param>
        /// <returns></returns>
        public static string GetPublicKeyFromPrivateKeyPkcs8(string privateKey)
        {
            var privateKeyInfo = PrivateKeyInfo.GetInstance(Asn1Object.FromByteArray(Base64.Decode(privateKey)));
            privateKey = Base64.ToBase64String(privateKeyInfo.ParsePrivateKey().GetEncoded());

            var instance = RsaPrivateKeyStructure.GetInstance(Base64.Decode(privateKey));
       
            var publicParameter = (AsymmetricKeyParameter)new RsaKeyParameters(false, instance.Modulus, instance.PublicExponent);

            var privateParameter = (AsymmetricKeyParameter)new RsaPrivateCrtKeyParameters(instance.Modulus, instance.PublicExponent, instance.PrivateExponent, instance.Prime1, instance.Prime2, instance.Exponent1, instance.Exponent2, instance.Coefficient);

            var keyPair = new AsymmetricCipherKeyPair(publicParameter, privateParameter);
            var subjectPublicKeyInfo = SubjectPublicKeyInfoFactory.CreateSubjectPublicKeyInfo(keyPair.Public);

            return Base64.ToBase64String(subjectPublicKeyInfo.GetEncoded());
        }

        /// <summary>
        /// 从Pkcs1私钥中提取公钥
        /// </summary>
        /// <param name="privateKey">Pkcs1私钥</param>
        /// <returns></returns>
        public static string GetPublicKeyFromPrivateKeyPkcs1(string privateKey)
        {
            var instance = RsaPrivateKeyStructure.GetInstance(Base64.Decode(privateKey));

            var publicParameter = (AsymmetricKeyParameter)new RsaKeyParameters(false, instance.Modulus, instance.PublicExponent);

            var privateParameter = (AsymmetricKeyParameter)new RsaPrivateCrtKeyParameters(instance.Modulus, instance.PublicExponent, instance.PrivateExponent, instance.Prime1, instance.Prime2, instance.Exponent1, instance.Exponent2, instance.Coefficient);

            var keyPair = new AsymmetricCipherKeyPair(publicParameter, privateParameter);
            var subjectPublicKeyInfo = SubjectPublicKeyInfoFactory.CreateSubjectPublicKeyInfo(keyPair.Public);

            return Base64.ToBase64String(subjectPublicKeyInfo.GetEncoded());
        }
    }
}
