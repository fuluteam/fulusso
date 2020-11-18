using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;
using Fulu.Core.Regular;
using Fulu.Passport.Domain.Component;

namespace Fulu.Passport.Domain.Models
{
    public class IdentityValidateInputDto
    {
        [RegularExpression(RegExp.PhoneNumber, ErrorMessage = "手机号码格式不正确")]
        public string Phone { get; set; }
        [Required]
        public string Code { get; set; }
    }
}
