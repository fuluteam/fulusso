using System.Text.RegularExpressions;
using System.Web;

namespace Fulu.Core.Regular
{
    /// <summary>
    /// 正则辅助类库
    /// </summary>
    public static class RegexHelper
    {
        /// <summary>
        ///     提取正则匹配结果
        /// </summary>
        /// <param name="source"></param>
        /// <param name="reg"></param>
        /// <returns></returns>
        public static string RegexValue(string source, string reg)
        {
            return Regex.Match(source, reg).Groups[1].Value.Trim();
        }

        /// <summary>
        ///     提取正则匹配结果
        /// </summary>
        /// <param name="source"></param>
        /// <param name="reg"></param>
        /// <param name="groupsIndex"></param>
        /// <returns></returns>
        public static string RegexValue(string source, string reg, int groupsIndex)
        {
            return Regex.Match(source, reg).Groups[groupsIndex].Value.Trim();
        }

        /// <summary>
        ///     压缩
        /// </summary>
        public static string Compress(string source)
        {
            var deCodeStr = HttpUtility.HtmlDecode(source);

            return !string.IsNullOrEmpty(deCodeStr) ? Regex.Replace(deCodeStr, "\r\n|\n|\t|  ", "") : "";
        }
    }

}
