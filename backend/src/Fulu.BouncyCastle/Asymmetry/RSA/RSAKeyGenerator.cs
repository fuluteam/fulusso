using System.IO;
using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.OpenSsl;
using Org.BouncyCastle.Pkcs;
using Org.BouncyCastle.Security;
using Org.BouncyCastle.Utilities.Encoders;
using Org.BouncyCastle.X509;

namespace Fulu.BouncyCastle
{
    public class RSAKeyGenerator
    {
        public static KeyParameter Pkcs1(int keySize, bool format=false)
        {
            var keyGenerator = GeneratorUtilities.GetKeyPairGenerator("RSA");
            keyGenerator.Init(new KeyGenerationParameters(new SecureRandom(), keySize));
        
            var keyPair = keyGenerator.GenerateKeyPair();

            var subjectPublicKeyInfo = SubjectPublicKeyInfoFactory.CreateSubjectPublicKeyInfo(keyPair.Public);
            var privateKeyInfo = PrivateKeyInfoFactory.CreatePrivateKeyInfo(keyPair.Private);
            
            if (!format)
            {
                return new KeyParameter
                {
                    PrivateKey = Base64.ToBase64String(privateKeyInfo.ParsePrivateKey().GetEncoded()),
                    PublicKey = Base64.ToBase64String(subjectPublicKeyInfo.GetEncoded())
                };
            }

            var rsaKey = new KeyParameter();
            using (var sw = new StringWriter())
            {
                var pWrt = new PemWriter(sw);
                pWrt.WriteObject(keyPair.Private);
                pWrt.Writer.Close();
                rsaKey.PrivateKey = sw.ToString();
            }

            using (var sw = new StringWriter())
            {
                var pWrt = new PemWriter(sw);
                pWrt.WriteObject(keyPair.Public);
                pWrt.Writer.Close();
                rsaKey.PublicKey = sw.ToString();
            }

            return rsaKey;
        }

        public static KeyParameter Pkcs8(int keySize, bool format=false)
        {
            var keyGenerator = GeneratorUtilities.GetKeyPairGenerator("RSA");
            keyGenerator.Init(new KeyGenerationParameters(new SecureRandom(), keySize));
            var keyPair = keyGenerator.GenerateKeyPair();

            var subjectPublicKeyInfo = SubjectPublicKeyInfoFactory.CreateSubjectPublicKeyInfo(keyPair.Public);
            var privateKeyInfo = PrivateKeyInfoFactory.CreatePrivateKeyInfo(keyPair.Private);

            if (!format)
            {
                return new KeyParameter
                {
                    PrivateKey = Base64.ToBase64String(privateKeyInfo.GetEncoded()),
                    PublicKey = Base64.ToBase64String(subjectPublicKeyInfo.GetEncoded())
                };
            }

            var rsaKey = new KeyParameter();
            using (var sw = new StringWriter())
            {
                var pWrt = new PemWriter(sw);
                var pkcs8 = new Pkcs8Generator(keyPair.Private);
                pWrt.WriteObject(pkcs8);
                pWrt.Writer.Close();
                rsaKey.PrivateKey = sw.ToString();
            }

            using (var sw = new StringWriter())
            {
                var pWrt = new PemWriter(sw);
                pWrt.WriteObject(keyPair.Public);
                pWrt.Writer.Close();
                rsaKey.PublicKey = sw.ToString();
            }

            return rsaKey;
        }
    }
}
