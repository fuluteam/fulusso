using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace Fulu.Passport.Domain.Models
{
    public class UnbindExternalUserInputDto
    {
        /// <summary>
        /// 第三方标识（qq、wechat），必填。
        /// </summary>
        [Required]
        public string LoginProvider { get; set; }
    }
}
