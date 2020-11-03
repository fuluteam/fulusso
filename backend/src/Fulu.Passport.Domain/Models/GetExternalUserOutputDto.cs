using System;
using System.Collections.Generic;
using System.Text;

namespace Fulu.Passport.Domain.Models
{
    public class GetExternalUserOutputDto
    {
        public string ProviderKey { get; set; }
        public string LoginProvider { get; set; }
        public string NickName { get; set; }
    }
}
