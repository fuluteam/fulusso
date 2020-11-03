using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace Fulu.Passport.Domain.Models
{
    public class BindExternalUserInputDto
    {
        /// <summary>
        /// 第三方用户标识（openid），必填。
        /// </summary>
        [Required]
        public string ProviderKey { get; set; }
        /// <summary>
        /// 第三方标识（qq、wechat），必填
        /// </summary>
        [Required]
        public string LoginProvider { get; set; }
        /// <summary>
        /// 昵称，选填。
        /// </summary>
        public string NickName { get; set; }
    }
}
