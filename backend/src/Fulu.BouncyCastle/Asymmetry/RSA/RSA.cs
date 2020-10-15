using System;
using System.Text;
using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Security;
using Org.BouncyCastle.Utilities.Encoders;

namespace Fulu.BouncyCastle
{
    public class RSA
    {
        public static byte[] Encrypt(byte[] data, AsymmetricKeyParameter parameters, string algorithm)
        {
            if (data == null)
            {
                throw new ArgumentNullException(nameof(data));
            }
            if (parameters == null)
            {
                throw new ArgumentNullException(nameof(parameters));
            }
            if (string.IsNullOrEmpty(algorithm))
            {
                throw new ArgumentNullException(nameof(algorithm));
            }

            var bufferedCipher = CipherUtilities.GetCipher(algorithm);
            bufferedCipher.Init(true, parameters);
            return bufferedCipher.DoFinal(data);
        }

        public static byte[] Decrypt(byte[] data, AsymmetricKeyParameter parameters, string algorithm)
        {
            if (data == null)
            {
                throw new ArgumentNullException(nameof(data));
            }
            if (parameters == null)
            {
                throw new ArgumentNullException(nameof(parameters));
            }
            if (string.IsNullOrEmpty(algorithm))
            {
                throw new ArgumentNullException(nameof(algorithm));
            }
            var bufferedCipher = CipherUtilities.GetCipher(algorithm);
            bufferedCipher.Init(false, parameters);
            return bufferedCipher.DoFinal(data);
        }

        public static string EncryptToBase64(string data, AsymmetricKeyParameter parameters, string algorithm)
        {
            return Base64.ToBase64String(Encrypt(Encoding.UTF8.GetBytes(data), parameters, algorithm));
        }

        public static string DecryptFromBase64(string data, AsymmetricKeyParameter parameters, string algorithm)
        {
            return Encoding.UTF8.GetString(Decrypt(Base64.Decode(data), parameters, algorithm));
        }

        public static string EncryptToHex(string data, AsymmetricKeyParameter parameters, string algorithm)
        {
            return Hex.ToHexString(Encrypt(Encoding.UTF8.GetBytes(data), parameters, algorithm));
        }

        public static string DecryptFromHex(string data, AsymmetricKeyParameter parameters, string algorithm)
        {
            return Encoding.UTF8.GetString(Decrypt(Hex.Decode(data), parameters, algorithm));
        }
    }
}
