using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace Fulu.Authentication.Models
{
    public class JwtToken
    {
        [JsonProperty("access_token")]
        public string AccessToken { get; set; }
        [JsonProperty("token_type")]
        public string TokenType { get; set; }
        [JsonProperty("expires_in")]
        public string ExpiresIn { get; set; }
        [JsonProperty("refresh_token")]
        public string RefreshToken { get; set; }

    }
}
