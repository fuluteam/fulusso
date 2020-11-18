using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;
using Fulu.Core.Regular;

namespace Fulu.Passport.Domain.Models
{
    public class ResetPasswordValidateInputDto
    {
        [RegularExpression(RegExp.PhoneNumber, ErrorMessage = "手机号码格式不正确")]
        public string Phone { get; set; }
        [Required]
        public string Code { get; set; }
    }
}
