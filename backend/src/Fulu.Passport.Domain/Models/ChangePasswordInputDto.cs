using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace Fulu.Passport.Domain.Models
{
    public class ChangePasswordInputDto
    {
        /// <summary>
        /// 验证码验证通过后，保存的ticket
        /// </summary>
        [Required]
        public string Ticket { get; set; }
        [Required]
        public string Password { get; set; }
    }
}
