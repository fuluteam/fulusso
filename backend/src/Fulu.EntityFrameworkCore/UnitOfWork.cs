using System;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;
using Dapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;

namespace Microsoft.EntityFrameworkCore
{
    /// <summary>
    /// Represents the default implementation of the <see cref="IUnitOfWork"/> 
    /// </summary>
    public class UnitOfWork : IUnitOfWork
    {
        private readonly DbContext _context;
        private bool _disposed = false;

        /// <summary>
        /// Initializes a new instance of the 
        /// </summary>
        /// <param name="context">The context.</param>
        public UnitOfWork(DbContext context)
        {
            _context = context;
        }


        /// <summary>
        /// 同步提交数据
        /// </summary>
        /// <returns>影响行数</returns>
        public int SaveChanges()
        {
            return _context.SaveChanges();
        }

        /// <summary>
        /// 异步提交数据
        /// </summary>
        /// <returns>影响行数</returns>
        public Task<int> SaveChangesAsync()
        {
            return _context.SaveChangesAsync();
        }

        /// <summary>
        /// 执行查询SQL语句
        /// SQL语句禁止拼接字符串，必须使用参数化SQL语句
        /// ag:await _unitOfWork.QueryAsync`Demo`("select id,name from school where id = @id", new { id = 20044 });
        /// </summary>
        /// <typeparam name="TEntity"></typeparam>
        /// <param name="sql">sql语句</param>
        /// <param name="param">参数</param>
        /// <param name="trans"></param>
        /// <returns></returns>
        public Task<IEnumerable<TEntity>> QueryAsync<TEntity>(string sql, object param = null, IDbContextTransaction trans = null) where TEntity : class
        {
            var conn = GetConnection();
            return conn.QueryAsync<TEntity>(sql, param, trans?.GetDbTransaction());

        }

        /// <summary>
        /// 执行提交SQL语句，自动提交
        /// SQL语句禁止拼接字符串，必须使用参数化SQL语句
        /// ag:await _unitOfWork.ExecuteAsync("update school set name =@name where id =@id", new { name = new Random().Next(10000).ToString(), id });
        /// </summary>
        /// <param name="sql"></param>
        /// <param name="param"></param>
        /// <param name="trans"></param>
        /// <returns></returns>
        public async Task<int> ExecuteAsync(string sql, object param, IDbContextTransaction trans = null)
        {
            var conn = GetConnection();
            return await conn.ExecuteAsync(sql, param, trans?.GetDbTransaction());

        }

        /// <summary>
        /// 执行提交SQL语句，自动提交
        /// SQL语句禁止拼接字符串，必须使用参数化SQL语句
        /// </summary>
        public async Task<T> ExecuteScalarAsync<T>(string sql, object param, IDbContextTransaction trans = null)
        {
            var conn = GetConnection();
            return await conn.ExecuteScalarAsync<T>(sql, param, trans?.GetDbTransaction());
        }

        //public async Task<PagedList<TEntity>> QueryPagedListAsync<TEntity>(int pageIndex, int pageSize, string pageSql, object pageSqlArgs = null) where TEntity : class
        //{
        //    if (pageSize < 1 || pageSize > 5000)
        //        throw new ArgumentOutOfRangeException(nameof(pageSize));
        //    if (pageIndex < 1)
        //        throw new ArgumentOutOfRangeException(nameof(pageIndex));

        //    var partedSql = PagingUtil.SplitSql(pageSql);
        //    ISqlAdapter sqlAdapter = null;
        //    if (_context.Database.IsMySql())
        //        sqlAdapter = new MysqlAdapter();
        //    if (_context.Database.IsSqlServer())
        //        sqlAdapter = new SqlServerAdapter();
        //    if (sqlAdapter == null)
        //        throw new Exception("不支持的数据库类型");
        //    pageSql = sqlAdapter.PagingBuild(ref partedSql, pageSqlArgs, (pageIndex - 1) * pageSize, pageSize);
        //    var sqlCount = PagingUtil.GetCountSql(partedSql);
        //    var conn = GetConnection();
        //    var totalCount = await conn.ExecuteScalarAsync<int>(sqlCount, pageSqlArgs);
        //    var items = await conn.QueryAsync<TEntity>(pageSql, pageSqlArgs);
        //    var pagedList = new PagedList<TEntity>(items.ToList(), pageIndex - 1, pageSize, totalCount);
        //    return pagedList;
        //}

        /// <summary>
        /// 开启事务
        /// </summary>
        /// <returns></returns>
        public IDbContextTransaction BeginTransaction()
        {
            return _context.Database.BeginTransaction();
        }



        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        /// <param name="disposing">The disposing.</param>
        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                    _context.Dispose();
                }
            }
            _disposed = true;
        }

        IDbConnection GetConnection()
        {
            return _context.Database.GetDbConnection();
        }
    }


}
