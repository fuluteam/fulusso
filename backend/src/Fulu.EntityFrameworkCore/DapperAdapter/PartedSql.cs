namespace Microsoft.EntityFrameworkCore.DapperAdapter
{
    /// <summary>
    /// 分解后的SQL信息
    /// </summary>
    public struct PartedSql
    {
        public string Raw;
        /// <summary>
        /// select部分（包含top x或distinct等修饰语句），例如：distinct Id, name
        /// </summary>
        public string Select;

        /// <summary>
        /// body部分（包含from与orderBy之前的内容），例如：tabName where Id = 123
        /// </summary>
        public string Body;

        /// <summary>
        /// order by部分，例如：Id Asc
        /// </summary>
        public string OrderBy;
    }
}
