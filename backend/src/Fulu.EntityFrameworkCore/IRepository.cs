using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore.ChangeTracking;

namespace Microsoft.EntityFrameworkCore
{
    /// <summary>
    /// This interface is implemented by all repositories to ensure implementation of fixed methods.
    /// </summary>
    /// <typeparam name="TEntity">Main Entity type this repository works on</typeparam>
    public interface IRepository<TEntity> where TEntity : class
    {

        #region Select/Get/Query

#if  NETSTANDARD2_0
        /// <summary>
        /// 根据主键获取实体
        /// </summary>
        /// <param name="id">主键</param>
        /// <returns>Entity</returns>
        Task<TEntity> GetByIdAsync(object id);
        /// <summary>
        /// 添加实体
        /// </summary>
        /// <param name="entity">Inserted entity</param>
        Task InsertAsync(TEntity entity);
#endif
#if NETSTANDARD2_1
       /// <summary>
        /// 根据主键获取实体
        /// </summary>
        /// <param name="id">主键</param>
        /// <returns>Entity</returns>
        ValueTask<TEntity> GetByIdAsync(object id);
        ValueTask<EntityEntry<TEntity>> InsertAsync(TEntity entity);
#endif

        /// <summary>
        /// 带跟踪的查询表达式，不执行数据库查询
        /// </summary>
        IQueryable<TEntity> Table { get; }

        /// <summary>
        /// 不带跟踪的查询表达式，不执行数据库查询，性能更高
        /// </summary>
        IQueryable<TEntity> TableNoTracking { get; }

        #endregion

        #region Insert
        /// <summary>
        /// 添加实体
        /// </summary>
        /// <param name="entity">Inserted entity</param>
        void Insert(TEntity entity);

        /// <summary>
        /// 添加实体集合
        /// </summary>
        /// <param name="entities">Inserted entity</param>
        void Insert(IEnumerable<TEntity> entities);
       

        /// <summary>
        /// 添加实体集合
        /// </summary>
        /// <param name="entities">Inserted entity</param>
        Task InsertAsync(IEnumerable<TEntity> entities);
        #endregion

        #region Update
        /// <summary>
        /// 全量更新实体
        /// </summary>
        /// <param name="entity">Entity</param>
        void Update(TEntity entity);

        /// <summary>
        /// 全量更新实体集合
        /// </summary>
        /// <param name="entities"></param>
        void Update(IEnumerable<TEntity> entities);

        /// <summary>
        /// 增量更新实体，不用先查询实体，实体对象必须包含主键
        /// </summary>
        /// <param name="entity"></param>
        /// <param name="properties">Action that can be used to change values of the entity</param>
        /// <returns>Updated entity</returns>
        void Update(TEntity entity, params Expression<Func<TEntity, object>>[] properties);
        #endregion

        #region Delete
        /// <summary>
        /// 删除实体
        /// </summary>
        /// <param name="entity">Entity to be deleted</param>
        void Delete(TEntity entity);


        /// <summary>
        /// 删除实体集合
        /// </summary>
        /// <param name="entities"></param>
        void Delete(IEnumerable<TEntity> entities);
        /// <summary>
        /// 根据主键删除实体
        /// </summary>
        /// <param name="id">Primary key of the entity</param>
        /// <param name="primaryKey">主键列名，默认为Id</param>
        void Delete(object id, string primaryKey = "Id");

        /// <summary>
        /// 根据labmda表达式删除实体集合
        /// </summary>
        /// <param name="predicate">A condition to filter entities</param>
        void Delete(Expression<Func<TEntity, bool>> predicate);
        #endregion

    }
}
