using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;
using Fulu.Core.Regular;

namespace Fulu.Passport.Domain.Models
{
    /// <summary>
    /// 
    /// </summary>
    public class SmsSendInputDto
    {
        /// <summary>
        /// 
        /// </summary>
        [Required, RegularExpression(RegExp.PhoneNumber, ErrorMessage = "手机号码格式不正确")]
        public string Phone { get; set; }
        [Required]
        public int Type { get; set; }
        /// <summary>
        /// 验证码客户端验证回调的票据
        /// </summary>
        [Required]
        public string Ticket { get; set; }
        /// <summary>
        /// 验证码客户端验证回调的随机串
        /// </summary>
        [Required]
        public string RandStr { get; set; }
    }
}
