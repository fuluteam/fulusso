using System;
using System.Collections.Generic;
using System.Text;

namespace Fulu.Authentication.Options
{
    public class AuthorizeTokenOptions
    {
        /// <summary>
        /// 接口鉴权地址
        /// </summary>
        public string AuthorizeEndpoint { get; set; }
        /// <summary>
        /// 客户端ID
        /// </summary>
        public string ClientId { get; set; }
        /// <summary>
        /// 客户端密钥
        /// </summary>
        public string ClientSecret { get; set; }
    }
}
