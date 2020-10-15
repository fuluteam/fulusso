using System.Collections.Generic;

namespace Microsoft.Extensions.Options
{
    /// <summary>
    /// 
    /// </summary>
    public class ServiceAuthorizeOptions
    {
        /// <summary>
        /// 客户端ID
        /// </summary>
        public string ClientId { get; set; }
        /// <summary>
        /// 客户端密钥
        /// </summary>
        public string ClientSecret { get; set; }
        /// <summary>
        /// 是否允许客户端令牌访问（设置为false时，禁止客户端令牌访问）
        /// </summary>
        public bool AllowClientToken { get; set; } = true;

        /// <summary>
        /// 是否允许用户令牌访问 （设置为false时，禁止用户令牌访问）
        /// </summary>
        public bool AllowUserToken { get; set; } = true;

        /// <summary>
        /// 是否开启客户端令牌验证（必须提供验证地址，否则为false）
        /// </summary>
        public bool OnClientValidate { get; set; } = false;

        /// <summary>
        /// 验证发行人
        /// </summary>
        public string ValidIssuer { get; set; } = "http://localhost:80";

        /// <summary>
        /// 是否验证发行人，默认为true
        /// </summary>
        public bool ValidateIssuer { get; set; } = true;
        /// <summary>
        /// 是否验证听众
        /// </summary>
        public bool ValidateAudience { get; set; } = true;
        /// <summary>
        /// 验证令牌的有效受众
        /// </summary>
        public IEnumerable<string> ValidAudiences { get; set; } = new[] { "api", "get_user_info" };
        public string HS256Key { get; set; }
        /// <summary>
        /// 获取或设置在进行OpenIdConnect调用时要使用的权限。
        /// </summary>
        public string Authority { get; set; }
    }
}
