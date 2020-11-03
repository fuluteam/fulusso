using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace Fulu.Passport.Domain.Models
{
    public class LoginByCodeBindModel
    {
        [Required]
        public string Phone { get; set; }
        [Required]
        public string Code { get; set; }
    }
}
