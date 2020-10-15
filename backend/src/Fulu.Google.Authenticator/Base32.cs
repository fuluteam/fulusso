using System;
using System.Collections.Generic;
using System.Text;

namespace Fulu.Google.Authenticator
{
    public class Base32
    {
        public static string Encode(byte[] data)
        {
            const int inByteSize = 8;
            const int outByteSize = 5;
            var alphabet = "ABCDEFGHIJKLMNOPQRSTUVWXYZ234567".ToCharArray();

            int i = 0, index = 0;
            var result = new StringBuilder((data.Length + 7) * inByteSize / outByteSize);

            while (i < data.Length)
            {
                var currentByte = data[i];

                /* Is the current digit going to span a byte boundary? */
                int digit;
                if (index > (inByteSize - outByteSize))
                {
                    var nextByte = (i + 1) < data.Length ? data[i + 1] : 0;

                    digit = currentByte & (0xFF >> index);
                    index = (index + outByteSize) % inByteSize;
                    digit <<= index;
                    digit |= nextByte >> (inByteSize - index);
                    i++;
                }
                else
                {
                    digit = (currentByte >> (inByteSize - (index + outByteSize))) & 0x1F;
                    index = (index + outByteSize) % inByteSize;
                    if (index == 0)
                        i++;
                }
                result.Append(alphabet[digit]);
            }

            return result.ToString();
        }
    }
}