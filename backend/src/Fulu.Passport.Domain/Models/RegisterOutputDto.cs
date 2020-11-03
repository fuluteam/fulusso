using System;
using System.Collections.Generic;
using System.Text;

namespace Fulu.Passport.Domain.Models
{
    public class RegisterOutputDto
    {
        public string OpenId { get; set; }

        public string UserName { get; set; }

        public string NickName { get; set; }

        public string Phone { get; set; }

        public string Gender { get; set; }

        public string Email { get; set; }
    }
}
