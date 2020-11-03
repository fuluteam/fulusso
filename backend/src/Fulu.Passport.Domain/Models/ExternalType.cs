using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;

namespace Fulu.Passport.Domain.Models
{
    public enum ExternalType
    {
        [Description("QQ")]
        QQ,
        [Description("微信")]
        WeChat,
        [Description("微博")]
        WeiBo,
        [Description("钉钉")]
        DingTalk,
        [Description("企业微信")]
        QyWeChat
    }
}
