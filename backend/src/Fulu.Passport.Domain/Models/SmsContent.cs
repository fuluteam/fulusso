using Fulu.Passport.Domain.Component;
using System;
using System.Collections.Generic;
using System.Text;

namespace Fulu.Passport.Domain.Models
{
    /// <summary>
    /// 
    /// </summary>
    public class SmsContent
    {
        /// <summary>
        /// 
        /// </summary>
        public string Code { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public ValidationType ValidationType { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string Content { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public int ExpiresMinute { get; set; }
    }
}
