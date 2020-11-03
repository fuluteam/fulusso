using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace Fulu.Passport.Domain.Models
{
    public class ResetPasswordInputDto
    {
        [Required]
        public string Ticket { get; set; }
        [Required]
        public string Password { get; set; }
    }
}
