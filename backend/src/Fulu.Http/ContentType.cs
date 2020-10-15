using System;
using System.Collections.Generic;
using System.Text;

namespace Fulu.Http
{
    public class ContentType
    {
        /// <summary>
        /// 表单类型Content-Type
        /// </summary>
        public const string CONTENT_TYPE_FORM = "application/x-www-form-urlencoded";
        /// <summary>
        /// 流类型Content-Type
        /// </summary>
        public const string CONTENT_TYPE_STREAM = "application/octet-stream";
        /// <summary>
        /// JSON类型Content-Type
        /// </summary>
        public const string CONTENT_TYPE_JSON = "application/json";
        /// <summary>
        /// XML类型Content-Type
        /// </summary>
        public const string CONTENT_TYPE_XML = "application/xml";
        /// <summary>
        /// 文本类型Content-Type
        /// </summary>
        public const string CONTENT_TYPE_TEXT = "application/text";
    }
}
