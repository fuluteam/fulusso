using System;
using System.Collections.Generic;
using System.Text;

namespace Fulu.Passport.Domain.Models
{
    public class ModifyClientInputDto
    {
        public string FullName { get; set; }
        public string ClientSecret { get; set; }
        public string HostUrl { get; set; }
        public string RedirectUri { get; set; }
        public string Description { get; set; }
    }
}
