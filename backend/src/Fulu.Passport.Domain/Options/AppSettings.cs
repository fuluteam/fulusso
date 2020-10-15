using System;
using System.Collections.Generic;
using System.Text;

namespace FuLu.Passport.Domain.Options
{
    public class AppSettings
    {
        public int ClientId { get; set; }
        public string ClientName { get; set; }
        public string ClientSecret { get; set; }
        public string X509RawCertData { get; set; }

        public string X509CertPwd { get; set; }

        public string PrivateKey { get; set; }

        public string AesKey { get; set; }
        public string AesIv { get; set; }

        public string HS256Key { get; set; }

        public bool DeveloperMode { get; set; }
    }
}
