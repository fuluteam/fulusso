using System.Linq;
using Fulu.Core.Extensions;
using Microsoft.Extensions.DependencyModel;

namespace Microsoft.EntityFrameworkCore
{
    /// <summary>
    /// 
    /// </summary>
    public class BaseDbContext : DbContext
    {
        /// <summary>
        /// 
        /// </summary>
        protected BaseDbContext()
        {
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="options"></param>
        public BaseDbContext(DbContextOptions options) : base(options)
        {
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="modelBuilder"></param>
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            var assemb = TypeExtensions.GetCurrentPathAssembly();
            foreach (var assembly in assemb)
            {
                var assName = assembly.FullName;
                var entityTypes = assembly.GetTypes()
                    .Where(type => !string.IsNullOrWhiteSpace(type.Namespace))
                    .Where(type => type.IsClass)
                    .Where(type => type.BaseType != null)
                    .Where(type => typeof(IEntity).IsAssignableFrom(type));

                foreach (var entityType in entityTypes)
                {
                    //  防止重复附加模型，否则会在生成指令中报错
                    if (modelBuilder.Model.FindEntityType(entityType) != null)
                        continue;
                    modelBuilder.Model.AddEntityType(entityType);
                }
            }
            base.OnModelCreating(modelBuilder);
        }


      
    }
}