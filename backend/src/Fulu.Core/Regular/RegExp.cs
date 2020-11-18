using System.Text.RegularExpressions;

namespace Fulu.Core.Regular
{
    public class RegExp
    {
        public const string PhoneNumber = "^(1[3-9])\\d{9}$";
        public const string UserName = "^[a-zA-Z0-9]{1}[a-zA-Z0-9_]{5,17}$";
        public const string Password = "^.{6,20}$";
        public const string Email = @"^[\w-]+(\.[\w-]+)*@[\w-]+(\.[\w-]+)+$";//w 英文字母或数字的字符串，和 [a-zA-Z0-9] 语法一样 
        public const string CHZN = "[\u4e00-\u9fa5]";
        public const string Letter = "^[a-zA-Z\\d]+$";
        public const string UpperCase = "^[A-Z\\d]+$";
        public const string Lowercase = "^[a-z\\d]+$";
    }
}
