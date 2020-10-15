using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace Fulu.Passport.Domain.Models
{
    public class RegisterInputDto
    {
        [RegularExpression(RegexConstance.IsPhone, ErrorMessage = "手机号码格式不正确")]
        public string Phone { get; set; }
        [Required]
        public string Password { get; set; }
        [StringLength(6, ErrorMessage = "输入短信验证码有误")]
        public string Code { get; set; }
        [MaxLength(18, ErrorMessage = "输入昵称太长，请确认不超过18个字符")]
        public string Nickname { get; set; }
    }
}
