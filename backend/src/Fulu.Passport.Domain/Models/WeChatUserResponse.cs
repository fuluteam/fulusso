using System;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json;

namespace Fulu.Passport.Domain.Models
{
    public class WeChatUserResponse
    {
        [JsonProperty("errcode")]
        public int Errcode { get; set; }
        [JsonProperty("errmsg")]
        public string Errmsg { get; set; }
        [JsonProperty("access_token")]
        public string AccessToken { get; set; }
        [JsonProperty("expires_in")]
        public int ExpiresIn { get; set; }
        [JsonProperty("refresh_token")]
        public string RefreshToken { get; set; }
        [JsonProperty("openid")]
        public string OpenId { get; set; }
        [JsonProperty("scope")]
        public string Scope { get; set; }
        [JsonProperty("unionid")]
        public string Unionid { get; set; }
    }
}
