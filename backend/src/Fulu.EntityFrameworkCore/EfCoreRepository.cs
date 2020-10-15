using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;

namespace Fulu.EntityFrameworkCore
{


    /// <summary>
    /// 
    /// </summary>
    /// <typeparam name="TEntity"></typeparam>
    public class EfCoreRepository<TEntity> : IRepository<TEntity>
        where TEntity : class
    {
        private readonly DbContext _context;
        private DbSet<TEntity> _entities;
        public EfCoreRepository(DbContext context)
        {
            _context = context;
        }
#if NETSTANDARD2_0
        public Task<TEntity> GetByIdAsync(object id)
        {
            if (id == null)
                throw new ArgumentNullException("id");

            return Entities.FindAsync(id);
        }
        public Task InsertAsync(TEntity entity)
        {
            if (entity == null)
                throw new ArgumentNullException("entity");

            return Entities.AddAsync(entity);
        }
#endif
#if  NETSTANDARD2_1
        public ValueTask<TEntity> GetByIdAsync(object id)
        {
            if (id == null)
                throw new ArgumentNullException("id");

            return Entities.FindAsync(id);
        }
        public ValueTask<EntityEntry<TEntity>> InsertAsync(TEntity entity)
        {
            if (entity == null)
                throw new ArgumentNullException("entity");

            return Entities.AddAsync(entity);
        }
#endif



        public void Insert(TEntity entity)
        {
            if (entity == null)
                throw new ArgumentNullException("entity");

            Entities.Add(entity);
        }

        public void Insert(IEnumerable<TEntity> entities)
        {
            if (!entities.Any())
                throw new ArgumentNullException("entities");

            Entities.AddRange(entities);
        }
        //public Task<int> BatchInsert(IEnumerable<TEntity> entities)
        //{
        //    var type = typeof(TEntity);
        //    var key = $"{nameof(ICH.King.MySqlBulk.BatchImport)}{type.Name}";
        //    var cache = CacheService.Get<List<EntityInfo>>(key);
        //    if (cache == null)
        //    {
        //        cache = new List<EntityInfo>();
        //        foreach (var propertyInfo in type.GetProperties())
        //        {
        //            var temp = new EntityInfo();
        //            temp.PropertyInfo = propertyInfo;
        //            temp.FieldName = propertyInfo.Name;
        //            var attr = propertyInfo.GetCustomAttribute<ColumnAttribute>();
        //            if (attr != null)
        //            {
        //                temp.FieldName = attr.Name;
        //            }
        //            cache.Add(temp);
        //        }
        //    }

        //}

        
        public Task InsertAsync(IEnumerable<TEntity> entities)
        {
            if (!entities.Any())
                throw new ArgumentNullException("entities");

            return Entities.AddRangeAsync(entities);
        }

        public void Update(TEntity entity)
        {
            if (entity == null)
                throw new ArgumentNullException("entity");

            Entities.Attach(entity);
            _context.Update(entity);
        }

        public void Update(IEnumerable<TEntity> entities)
        {
            if (!entities.Any())
                throw new ArgumentNullException("entities");

            _context.UpdateRange(entities);

        }

        public void Update(TEntity entity, params Expression<Func<TEntity, object>>[] properties)
        {
            //AttachIfNot(entity);
            //_context.Entry(entity).State = EntityState.Unchanged;
            foreach (var property in properties)
            {
                var propertyName = property.Name;
                if (string.IsNullOrEmpty(propertyName))
                {
                    propertyName = GetPropertyName(property.Body.ToString());
                }
                _context.Entry(entity).Property(propertyName).IsModified = true;
            }
        }

        string GetPropertyName(string str)
        {
            return str.Split(',')[0].Split('.')[1];
        }

        public void Delete(TEntity entity)
        {
            if (entity == null)
                throw new ArgumentNullException("entity");

            _context.Remove(entity);
        }


        public void Delete(IEnumerable<TEntity> entities)
        {
            if (!entities.Any())
                throw new ArgumentNullException("entities");

            _context.RemoveRange(entities);

        }

        public void Delete(object id, string primaryKey)
        {
            if (id == null)
                throw new ArgumentNullException("id");

            var entity = Activator.CreateInstance<TEntity>();

            var item = entity.GetType().GetProperties().FirstOrDefault(x => x.Name == primaryKey);
            if (item == null)
            {
                throw new ArgumentNullException("entity not has primaryKey");
            }

            item.SetValue(entity, id);
            Delete(entity);

        }

        public void Delete(Expression<Func<TEntity, bool>> predicate)
        {
            if (predicate == null)
                throw new ArgumentNullException("predicate");
            _context.RemoveRange(Entities.Where(predicate));
        }

#region Properties

        /// <summary>
        /// Gets a table
        /// </summary>
        public virtual IQueryable<TEntity> Table => Entities;

        /// <summary>
        /// Gets a table with "no tracking" enabled (EF feature) Use it only when you load record(s) only for read-only operations
        /// </summary>
        public virtual IQueryable<TEntity> TableNoTracking => Entities.AsNoTracking();

        /// <summary>
        /// Gets an entity set
        /// </summary>
        protected virtual DbSet<TEntity> Entities => _entities ?? (_entities = _context.Set<TEntity>());

#endregion
        protected virtual void AttachIfNot(TEntity entity)
        {
            var entry = _context.ChangeTracker.Entries().FirstOrDefault(ent => ent.Entity == entity);
            if (entry != null)
            {
                return;
            }
            _context.Attach(entity);
        }
    }
}
