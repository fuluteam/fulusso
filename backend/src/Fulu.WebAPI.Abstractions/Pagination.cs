namespace Fulu.WebAPI.Abstractions
{
    /// <summary>
    /// 分页参数
    /// </summary>
    public class Pagination : Statistic
    {
        /// <summary>
        /// 每页行数
        /// </summary>
        public int PageSize { get; set; }
        /// <summary>
        /// 当前页
        /// </summary>
        public int Current { get; set; }

        /// <summary>
        /// 总页数
        /// </summary>
        public int PageTotal
        {
            get
            {
                if (Total > 0)
                {
                    return Total % this.PageSize == 0 ? Total / this.PageSize : Total / this.PageSize + 1;
                }
                return 0;
            }
        }
    }
}
