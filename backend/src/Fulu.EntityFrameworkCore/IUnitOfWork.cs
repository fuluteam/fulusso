using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore.Storage;

namespace Microsoft.EntityFrameworkCore
{
    /// <summary>
    /// Defines the interface(s) for unit of work.
    /// </summary>
    public interface IUnitOfWork : IDisposable
    {
        /// <summary>
        /// Saves all changes made in this context to the database.
        /// </summary>
        /// <returns>The number of state entries written to the database.</returns>
        int SaveChanges();

        /// <summary>
        /// Asynchronously saves all changes made in this unit of work to the database.
        /// </summary>
        /// <returns>A <see cref="Task{TResult}"/> that represents the asynchronous save operation. The task result contains the number of state entities written to database.</returns>
        Task<int> SaveChangesAsync();

        #region command sql

        /// <summary>
        /// 执行查询SQL语句
        /// SQL语句禁止拼接字符串，必须使用参数化SQL语句
        /// ag:await _unitOfWork.QueryAsync`Demo`("select id,name from school where id = @id", new { id = 1 });
        /// </summary>
        /// <typeparam name="TEntity"></typeparam>
        /// <param name="sql">sql语句</param>
        /// <param name="param">参数</param>
        /// <param name="trans"></param>
        /// <returns></returns>
        Task<IEnumerable<TEntity>> QueryAsync<TEntity>(string sql, object param = null, IDbContextTransaction trans = null) where TEntity : class;

        /// <summary>
        /// 执行提交SQL语句，自动提交，非事务
        /// SQL语句禁止拼接字符串，必须使用参数化SQL语句
        /// ag:await _unitOfWork.ExecuteAsync("update school set name =@name where id =@id", new { name = "", id=1 });
        /// </summary>
        /// <param name="sql"></param>
        /// <param name="param"></param>
        /// <param name="trans"></param>
        /// <returns></returns>
        Task<int> ExecuteAsync(string sql, object param, IDbContextTransaction trans = null);
        /// <summary>
        /// 执行提交SQL语句，自动提交，非事务
        /// SQL语句禁止拼接字符串，必须使用参数化SQL语句
        /// </summary>
        Task<T> ExecuteScalarAsync<T>(string sql, object param, IDbContextTransaction trans = null);

        //Task<PagedList<TEntity>> QueryPagedListAsync<TEntity>(int pageIndex, int pageSize, string pageSql, object pageSqlArgs = null)
        //    where TEntity : class;
        #endregion

        #region 事务
        /// <summary>
        /// 开启事务
        /// </summary>
        /// <returns></returns>
        IDbContextTransaction BeginTransaction();

        #endregion
    }
}
