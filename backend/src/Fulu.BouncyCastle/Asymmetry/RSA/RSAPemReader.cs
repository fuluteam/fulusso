using System.IO;
using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.OpenSsl;
using Org.BouncyCastle.Pkcs;
using Org.BouncyCastle.Utilities.Encoders;
using Org.BouncyCastle.X509;

namespace Fulu.BouncyCastle
{
    public class RSAPemReader
    {
        public static string ReadPkcs1PrivateKey(string text)
        {
            if (!text.StartsWith("-----BEGIN RSA PRIVATE KEY-----"))
            {
                return text;
            }

            using (var reader = new StringReader(text))
            {
                var pr = new PemReader(reader);
                var keyPair = pr.ReadObject() as AsymmetricCipherKeyPair;
                pr.Reader.Close();

                var privateKeyInfo = PrivateKeyInfoFactory.CreatePrivateKeyInfo(keyPair?.Private);
                return Base64.ToBase64String(privateKeyInfo.ParsePrivateKey().GetEncoded());
            }
        }

        public static string ReadPkcs8PrivateKey(string text)
        {
            if (!text.StartsWith("-----BEGIN PRIVATE KEY-----"))
            {
                return text;
            }

            using (var reader = new StringReader(text))
            {
                var pr = new PemReader(reader);
                var akp = pr.ReadObject() as AsymmetricKeyParameter; ;
                pr.Reader.Close();
                return Base64.ToBase64String(PrivateKeyInfoFactory.CreatePrivateKeyInfo(akp).GetEncoded());
            }
        }

        public static string ReadPublicKey(string text)
        {
            using (var reader = new StringReader(text))
            {
                var pr = new PemReader(reader);
                var keyPair = pr.ReadObject() as AsymmetricCipherKeyPair;
                pr.Reader.Close();

                var subjectPublicKeyInfo = SubjectPublicKeyInfoFactory.CreateSubjectPublicKeyInfo(keyPair?.Public);
                return Base64.ToBase64String(subjectPublicKeyInfo.GetEncoded());
            }
        }
    }
}
