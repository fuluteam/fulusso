using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace FuLu.Passport.Domain.Models
{
    public class UserLoginInputDto
    {
        [Required(ErrorMessage = "必填")]
        [StringLength(20, MinimumLength = 4, ErrorMessage = "{2}到{1}个字符")]
        public string UserName { get; set; }
        [StringLength(1000, MinimumLength = 6, ErrorMessage = "{2}到{1}个字符")]
        [DataType(DataType.Password)]
        [Required(ErrorMessage = "必填")]
        public string Password { get; set; }
        public bool RememberMe { get; set; }

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
