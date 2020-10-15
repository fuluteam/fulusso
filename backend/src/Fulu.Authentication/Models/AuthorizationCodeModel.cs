using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace Fulu.Authentication.Models
{
    public class AuthorizationCodeModel
    {
        /// <summary>
        /// 授权码
        /// </summary>
        [Required]
        public string Code { get; set; }
        /// <summary>
        /// 状态值
        /// </summary>
        [Required]
        public string State { get; set; }
        /// <summary>
        /// 重定向URI
        /// </summary>
        [Required]
        public string RedirectUri { get; set; }
    }

}
