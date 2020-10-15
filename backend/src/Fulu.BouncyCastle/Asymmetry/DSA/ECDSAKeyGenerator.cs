using Org.BouncyCastle.Asn1.Sec;
using Org.BouncyCastle.Crypto.Parameters;
using Org.BouncyCastle.Pkcs;
using Org.BouncyCastle.Security;
using Org.BouncyCastle.Utilities.Encoders;
using Org.BouncyCastle.X509;

namespace Fulu.BouncyCastle
{
    public class ECDSAKeyGenerator
    {
        /// <summary>
        /// 生成ECDSA密钥对（secp256k1是比特币椭圆曲线）
        /// </summary>
        /// <returns></returns>
        public static KeyParameter Generator()
        {
            var keyGen = GeneratorUtilities.GetKeyPairGenerator("ECDSA");
            keyGen.Init(new ECKeyGenerationParameters(SecObjectIdentifiers.SecP256k1, new SecureRandom()));

            var kp = keyGen.GenerateKeyPair();

            var subjectPublicKeyInfo = SubjectPublicKeyInfoFactory.CreateSubjectPublicKeyInfo(kp.Public);
            var privateKeyInfo = PrivateKeyInfoFactory.CreatePrivateKeyInfo(kp.Private);

            return new KeyParameter
            {
                PrivateKey = Base64.ToBase64String(privateKeyInfo.GetEncoded()),
                PublicKey = Base64.ToBase64String(subjectPublicKeyInfo.GetEncoded())
            };
        }
    }
}
