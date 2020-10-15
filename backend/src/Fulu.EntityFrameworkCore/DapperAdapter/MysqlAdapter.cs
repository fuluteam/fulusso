namespace Microsoft.EntityFrameworkCore.DapperAdapter
{
    /// <summary>
    /// 
    /// </summary>
    public class MySqlAdapter : ISqlAdapter
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="partedSql"></param>
        /// <param name="args"></param>
        /// <param name="skip"></param>
        /// <param name="take"></param>
        /// <returns></returns>
        public virtual string PagingBuild(ref PartedSql partedSql, object args, long skip, long take)
        {
            var pageSql = $"{partedSql.Raw} LIMIT {take} OFFSET {skip}";
            return pageSql;
        }

    }
}
