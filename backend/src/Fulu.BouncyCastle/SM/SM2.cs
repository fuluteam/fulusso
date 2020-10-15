using Org.BouncyCastle.Crypto.Engines;
using Org.BouncyCastle.Crypto.Generators;
using Org.BouncyCastle.Crypto.Parameters;
using Org.BouncyCastle.Math;
using Org.BouncyCastle.Math.EC;
using Org.BouncyCastle.Pkcs;
using Org.BouncyCastle.Security;
using Org.BouncyCastle.Utilities;
using Org.BouncyCastle.Utilities.Encoders;
using Org.BouncyCastle.X509;

namespace Fulu.BouncyCastle
{
    public class SM2
    {
        //SM2使用素数域256位椭圆曲线
        //曲线参数
        private string ECC_P = "FFFFFFFEFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFF00000000FFFFFFFFFFFFFFFF";  //素数p
        private string ECC_A = "FFFFFFFEFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFF00000000FFFFFFFFFFFFFFFC";  //系数a
        private string ECC_B = "28E9FA9E9D9F5E344D5A9E4BCF6509A7F39789F515AB8F92DDBCBD414D940E93";  //系数b
        private string ECC_N = "FFFFFFFEFFFFFFFFFFFFFFFFFFFFFFFF7203DF6B21C6052B53BBF40939D54123";  //阶
        private string ECC_GX = "32C4AE2C1F1981195F9904466A39C9948FE30BBFF2660BE1715A4589334C74C7"; //坐标xG
        private string ECC_GY = "BC3736A2F4F6779C59BDCEE36B692153D0A9877CC62A474002DF32E52139F0A0"; //坐标yG

        public KeyParameter KeyGenerator()
        {
            var SM2_ECC_P = new BigInteger(ECC_P, 16);
            var SM2_ECC_A = new BigInteger(ECC_A, 16);
            var SM2_ECC_B = new BigInteger(ECC_B, 16);
            var SM2_ECC_N = new BigInteger(ECC_N, 16);
            var SM2_ECC_H = BigInteger.One;
            var SM2_ECC_GX = new BigInteger(ECC_GX, 16);
            var SM2_ECC_GY = new BigInteger(ECC_GY, 16);

            ECCurve curve = new FpCurve(SM2_ECC_P, SM2_ECC_A, SM2_ECC_B, SM2_ECC_N, SM2_ECC_H);

            var g = curve.CreatePoint(SM2_ECC_GX, SM2_ECC_GY);
            var domainParams = new ECDomainParameters(curve, g, SM2_ECC_N);

            var keyPairGenerator = new ECKeyPairGenerator();

            var aKeyGenParams = new ECKeyGenerationParameters(domainParams, new SecureRandom());

            keyPairGenerator.Init(aKeyGenParams);

            var aKp = keyPairGenerator.GenerateKeyPair();

            var subjectPublicKeyInfo = SubjectPublicKeyInfoFactory.CreateSubjectPublicKeyInfo(aKp.Public);
            var privateKeyInfo = PrivateKeyInfoFactory.CreatePrivateKeyInfo(aKp.Private);

            var pk =Hex.ToHexString(privateKeyInfo.GetEncoded());

            return new KeyParameter
            {
                PrivateKey = Base64.ToBase64String(privateKeyInfo.GetEncoded()),
                PublicKey = Base64.ToBase64String(subjectPublicKeyInfo.GetEncoded())
            };
        }

        public byte[] Encrypt(string s, ECPublicKeyParameters aPub)
        {
            var sm2Engine = new SM2Engine();

            var m = Strings.ToByteArray(s);

            sm2Engine.Init(true, new ParametersWithRandom(aPub, new SecureRandom()));

            return sm2Engine.ProcessBlock(m, 0, m.Length);
        }

        public string Decrypt(byte[] enc, ECPrivateKeyParameters aPriv)
        {
            var sm2Engine = new SM2Engine();

            sm2Engine.Init(false, aPriv);

            var dec = sm2Engine.ProcessBlock(enc, 0, enc.Length);

            return Strings.FromByteArray(dec);
        }
    }
}
