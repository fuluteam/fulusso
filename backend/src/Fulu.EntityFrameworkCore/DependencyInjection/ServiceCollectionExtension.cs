using System;
using Fulu.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class ServiceCollectionExtension
    {
   
        public static IServiceCollection AddEntityFrameworkCore<T>(this IServiceCollection services) where T : DbContext
        {
            services.AddScoped<DbContext, T>();
            services.AddScoped<IUnitOfWork, UnitOfWork>();
            services.AddScoped(typeof(IRepository<>), typeof(EfCoreRepository<>));
            return services;

        }


        public static IServiceCollection AddEntityFrameworkCore(this IServiceCollection services, Action<DbContextOptionsBuilder> options)
        {
            services.AddDbContext<BaseDbContext>(options);
            services.AddScoped<DbContext, BaseDbContext>();
            services.AddScoped<IUnitOfWork, UnitOfWork>();
            services.AddScoped(typeof(IRepository<>), typeof(EfCoreRepository<>));
            return services;
        }

        public static IServiceCollection AddEntityFrameworkCore<T>(this IServiceCollection services, Action<DbContextOptionsBuilder> options) where T : BaseDbContext
        {
            services.AddDbContext<T>(options);
            services.AddScoped<DbContext, T>();
            services.AddScoped<IUnitOfWork, UnitOfWork>();
            services.AddScoped(typeof(IRepository<>), typeof(EfCoreRepository<>));
            return services;
        }
    }
}