using System;
using System.Collections.Generic;
using System.Text;

//密码组合：		
//包含大写字母或小写字母
//大小写字母混合
//数字
//符号

//密码得分规则：
//以上4种密码组合，每包含一种 + 20分
//密码长度>=8位 +20分	
//密码长度>=6 & 密码长度<8位 +10分			
//密码长度<6位，总得分0分

//密码强度等级
//100分 极高
//>=80	高
//>=60	良好
//>=40	差
//<40	过于简单

namespace Fulu.Core
{
    /// <summary>
    /// 密码强度检测
    /// </summary>
    public class PasswordStrength
    {

    }
}
