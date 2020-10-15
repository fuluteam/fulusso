using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.EntityFrameworkCore.Metadata.Internal;

namespace Fulu.Passport.Domain.Options
{
    public class CaptchaOptions
    {
        public string AppId { get; set; }

        public string AppSecretKey { get; set; }
    }
}
