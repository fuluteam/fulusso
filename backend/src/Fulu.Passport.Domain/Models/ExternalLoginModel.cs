using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace Fulu.Passport.Domain.Models
{
    public class ExternalLoginModel
    {
        [Required]
        public string Provider { get; set; }

        public string ReturnUrl { get; set; }
    }
}
