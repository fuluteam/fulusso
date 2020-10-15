using Microsoft.AspNetCore.Authorization;
using System.Collections.Generic;

namespace Microsoft.AspNetCore.Authorization
{
    /// <summary>
    /// 
    /// </summary>
    public class AuthorizeRequirement : IAuthorizationRequirement
    {
        /// <summary>
        /// 是否允许客户端令牌访问（设置为false时，禁止客户端令牌访问）
        /// </summary>
        public bool AllowClientToken { get; set; } = true;
        /// <summary>
        /// 是否允许用户令牌访问 （设置为false时，禁止用户令牌访问）
        /// </summary>
        public bool AllowUserToken { get; set; } = true;
        /// <summary>
        /// 验证发行人
        /// </summary>
        public string ValidIssuer { get; set; }

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
        /// <summary>
        /// 获取或设置在进行OpenIdConnect调用时要使用的权限。
        /// </summary>
        public string Authority { get; set; }
    }
}
