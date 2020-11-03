using System;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json;

namespace Fulu.Passport.Domain.Models
{
    public class DingTalkUserResponse
    {
        public int Errcode { get; set; }
        public string Errmsg { get; set; }
        [JsonProperty("user_info")]
        public UserInfo UserInfo { get; set; }
    }

    public class UserInfo
    {
        public string Nick { get; set; }

        public string Openid { get; set; }
        public string Unionid { get; set; }
    }
}
