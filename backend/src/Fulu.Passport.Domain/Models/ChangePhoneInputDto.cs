using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace Fulu.Passport.Domain.Models
{
    public class ChangePhoneInputDto
    {
        [Required, RegularExpression(RegexConstance.IsPhone, ErrorMessage = "手机号码格式不正确")]
        public string Phone { get; set; }
        [Required]
        public string Code { get; set; }

        /// <summary>
        /// 验证码验证通过后，保存的ticket
        /// </summary>
        [Required]
        public string Ticket { get; set; }
    }
}
