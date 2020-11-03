using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace Fulu.Passport.Domain.Models
{
    public class GetExternalUserInputDto
    {
        /// <summary>
        /// 第三方用户标识（openid），必填。
        /// </summary>
        [Required]
        public string ProviderKey { get; set; }
    }
}
