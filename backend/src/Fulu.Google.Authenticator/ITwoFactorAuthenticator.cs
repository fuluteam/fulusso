using System;
using System.Collections.Generic;
using System.Text;

namespace Fulu.Google.Authenticator
{
    public interface ITwoFactorAuthenticator
    {
        SetupCode GenerateSetupCode(string issuer, string accountName, string accountSecretKey, int qrPixelsPerModule);
        [Obsolete]
        SetupCode GenerateSetupCode(string issuer, string accountName, byte[] accountSecretKey, int qrPixelsPerModule);

        string GeneratePinAtInterval(string accountSecretKey, long counter, int digits = 6);

        bool ValidateTwoFactorPin(string accountSecretKey, string twoFactorCodeFromClient);

        bool ValidateTwoFactorPin(string accountSecretKey, string twoFactorCodeFromClient, TimeSpan timeTolerance);

        string GetCurrentPin(string accountSecretKey);

        string GetCurrentPin(string accountSecretKey, DateTime now);

        string[] GetCurrentPins(string accountSecretKey);

        string[] GetCurrentPins(string accountSecretKey, TimeSpan timeTolerance);
    }
}
