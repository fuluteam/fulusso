using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Runtime.Serialization;
using System.Text;

namespace Fulu.Passport.Domain.Component
{
    public enum ValidationType
    {
        UnKnow = 0,
        /// <summary>
        /// 注册
        /// </summary>
        Register = 1,
        /// <summary>
        /// 登录
        /// </summary>
        Login = 2,
        /// <summary>
        ///  通过手机号找回密码
        /// </summary>
       ResetPassword = 3,
        /// <summary>
        /// 修改手机号
        /// </summary>
        ChangePhoneNo = 4,
        /// <summary>
        /// 验证
        /// </summary>
        Validate = 5
    }
}
