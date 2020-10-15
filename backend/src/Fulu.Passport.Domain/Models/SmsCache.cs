using System;
using System.Collections.Generic;
using System.Text;

namespace Fulu.Passport.Domain.Models
{
    public class SmsCache
    {
        public DateTime StartTime { get; set; }

        public int ExpiresMinute { get; set; }

        public string Code { get; set; }

        public int ValidateCount { get; set; }
    }
}
