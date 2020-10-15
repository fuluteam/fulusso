using System;
using System.Collections.Generic;
using System.Text;

namespace FuLu.Passport.Domain.Models
{
    public enum UserLoginModel
    {
        /// <summary>
        /// 账号密码
        /// </summary>
        Password,
        /// <summary>
        /// 短信
        /// </summary>
        SmsCode,
        /// <summary>
        /// 接口（oauth2.0 password）
        /// </summary>
        Interface,
        /// <summary>
        /// 第三方
        /// </summary>
        External
    }
}
