using Org.BouncyCastle.Crypto.Generators;
using Org.BouncyCastle.Crypto.Parameters;
using Org.BouncyCastle.Pkcs;
using Org.BouncyCastle.Security;
using Org.BouncyCastle.Utilities.Encoders;
using Org.BouncyCastle.X509;

namespace Fulu.BouncyCastle
{
    public class DSAKeyGenerator
    {

        /// <summary>
        /// DSA密钥对生成
        /// </summary>
        /// <param name="size">size must be from 512 - 1024 and a multiple of 64</param>
        /// <returns></returns>
        public static KeyParameter Generator(int size = 1024)
        {
            var pGen = new DsaParametersGenerator();
            pGen.Init(size, 80, new SecureRandom());
            var dsaParams = pGen.GenerateParameters();
            var kgp = new DsaKeyGenerationParameters(new SecureRandom(), dsaParams);
            var kpg = GeneratorUtilities.GetKeyPairGenerator("DSA");
            kpg.Init(kgp);

            var kp = kpg.GenerateKeyPair();

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
