using System.IO;
using Org.BouncyCastle.OpenSsl;

namespace Fulu.BouncyCastle
{
    public class RSAPemWriter
    {
        public static string WritePkcs1PrivateKey(string privateKey)
        {
            if (privateKey.StartsWith("-----BEGIN RSA PRIVATE KEY-----"))
            {
                return privateKey;
            }

            var akp = AsymmetricKeyUtilities.GetAsymmetricKeyParameterFormPrivateKey(privateKey);
            using (var sw = new StringWriter())
            {
                var pWrt = new PemWriter(sw);
                pWrt.WriteObject(akp);
                pWrt.Writer.Close();
                return sw.ToString();
            }
        }

        public static string WritePkcs8PrivateKey(string privateKey)
        {
            if (privateKey.StartsWith("-----BEGIN PRIVATE KEY-----"))
            {
                return privateKey;
            }

            var akp = AsymmetricKeyUtilities.GetAsymmetricKeyParameterFormAsn1PrivateKey(privateKey);

            using (var sw = new StringWriter())
            {
                var pWrt = new PemWriter(sw);
                var pkcs8 = new Pkcs8Generator(akp);
                pWrt.WriteObject(pkcs8);
                pWrt.Writer.Close();
                return sw.ToString();
            }
        }

        public static string WritePublicKey(string publicKey)
        {
            if (publicKey.StartsWith("-----BEGIN PUBLIC KEY-----"))
            {
                return publicKey;
            }
            var akp = AsymmetricKeyUtilities.GetAsymmetricKeyParameterFormPublicKey(publicKey);
            using (var sw = new StringWriter())
            {
                var pWrt = new PemWriter(sw);
                pWrt.WriteObject(akp);
                pWrt.Writer.Close();
                return sw.ToString();
            }
        }
    }
}
