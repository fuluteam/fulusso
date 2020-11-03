using System;
using System.Collections.Generic;
using System.Text;

namespace Fulu.Passport.Domain.Models
{
    public class GetUserInfoOutput
    {
        public string UserId { get; set; }

        public string OpenId { get; set; }

        public string NickName { get; set; }

        public string Phone { get; set; }
    }
}
