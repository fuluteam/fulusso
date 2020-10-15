using System;
using System.Linq;
using System.Reflection;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Fulu.AutoDI
{
    public static class InjectionServiceCollectionExtensions
    {
        /// <summary>
        /// 通过设置注入选项自动注入。
        /// </summary>
        /// <param name="services"></param>
        /// <param name="configure"></param>
        /// <returns></returns>
        public static IServiceCollection AutoDI(this IServiceCollection services, params Action<InjectionOption>[] configure)
        {
            var allAssemblies = TypeExtensions.GetCurrentPathAssembly();
            if (services == null)
                throw new ArgumentNullException(nameof(services));
            if (configure == null)
                throw new ArgumentNullException(nameof(configure));
            foreach (var option in configure)
            {
                var optionValue = new InjectionOption();
                option(optionValue);
                var libs = allAssemblies.Where(x => x.FullName.Contains(optionValue.LibPrefix));
                foreach (var lib in libs)
                {
                    var assTypes = lib.GetTypes();
                    foreach (var matchName in optionValue.MatchNames)
                    {
                        var implements = assTypes.Where(type => type.FullName != null && type.FullName.Contains(matchName));
                        foreach (var type in implements)
                        {
                            if (type.IsAbstract)
                            {
                                continue;
                            }
                            var interfaceTypes = type.GetInterface($"I{type.Name}");
                            if (interfaceTypes != null)
                            {
                                ServiceDescriptor serviceDescriptor = new ServiceDescriptor(interfaceTypes, type, optionValue.Lifetime);
                                if (!services.Contains(serviceDescriptor))
                                    services.Add(serviceDescriptor);
                            }
                        }
                    }
                }
            }
            return services;
        }
        /// <summary>
        /// 继承系统默认接口IAutoDIable自动注入。
        /// </summary>
        /// <param name="services"></param>
        /// <returns></returns>
        public static IServiceCollection AutoDI(this IServiceCollection services)
        {
            return services.AutoDI(typeof(IAutoDIable));
        }
        /// <summary>
        /// 继承自定义泛型接口自动注入。
        /// </summary>
        /// <param name="services"></param>
        /// <param name="baseType"></param>
        /// <returns></returns>
        public static IServiceCollection AutoDI(this IServiceCollection services, Type baseType)
        {
            var allAssemblies = TypeExtensions.GetCurrentPathAssembly();
            foreach (var assembly in allAssemblies)
            {
                var classz = assembly.GetTypes()
                    .Where(type => type.IsClass
                                   && type.BaseType != null
                                   && type.HasImplementedRawGeneric(baseType));

                foreach (var type in classz)
                {
                    var interfaces = type.GetInterfaces();

                    #region 获取生命周期
                    ServiceLifetime lifetime = ServiceLifetime.Scoped;
                    var autoDIableType = typeof(IAutoDIable);
                    var lifetimeInterface = interfaces.FirstOrDefault(x =>
                        x.FullName != baseType.FullName
                        && x.FullName != autoDIableType.FullName
                        && x.Name.EndsWith("AutoDIable")
                    );
                    if (lifetimeInterface != null)
                    {
                        switch (lifetimeInterface.Name)
                        {
                            case nameof(IScopedAutoDIable):
                                lifetime = ServiceLifetime.Scoped;
                                break;
                            case nameof(ITransientAutoDIable):
                                lifetime = ServiceLifetime.Transient;
                                break;
                            case nameof(ISingletonAutoDIable):
                                lifetime = ServiceLifetime.Singleton;
                                break;
                        }
                    }
                    #endregion
                    var interfaceType = interfaces.FirstOrDefault(x => x.Name == $"I{type.Name}");
                    if (interfaceType == null)
                    {
                        interfaceType = type;
                    }
                    ServiceDescriptor serviceDescriptor = new ServiceDescriptor(interfaceType, type, lifetime);
                    if (!services.Contains(serviceDescriptor))
                    {
                        services.TryAdd(serviceDescriptor);
                    }
                }
            }
            return services;
        }
        /// <summary>
        /// 继承自定义接口自动注入。
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="services"></param>
        /// <returns></returns>
        public static IServiceCollection AutoDI<T>(this IServiceCollection services)
        {
            return services.AutoDI(typeof(T));
        }

        /// <summary>
        /// 继承自定义泛型接口自动注入。
        /// </summary>
        /// <param name="services"></param>
        /// <param name="assemblies"></param>
        /// <returns></returns>
        public static IServiceCollection AutoDI(this IServiceCollection services, params Assembly[] assemblies)
        {
            var baseType = typeof(IAutoDIable);
            foreach (var assembly in assemblies)
            {
                var handlerTypes = assembly.GetTypes().Where(x => x.IsClass && typeof(IAutoDIable).IsAssignableFrom(x));
                foreach (var type in handlerTypes)
                {
                    var interfaces = type.GetInterfaces();

                    #region 获取生命周期
                    var lifetime = ServiceLifetime.Scoped;
                    var lifetimeInterface = interfaces.FirstOrDefault(x => x.FullName != baseType.FullName);
                    if (lifetimeInterface != null)
                    {
                        switch (lifetimeInterface.Name)
                        {
                            case nameof(IScopedAutoDIable):
                                lifetime = ServiceLifetime.Scoped;
                                break;
                            case nameof(ITransientAutoDIable):
                                lifetime = ServiceLifetime.Transient;
                                break;
                            case nameof(ISingletonAutoDIable):
                                lifetime = ServiceLifetime.Singleton;
                                break;
                        }
                    }
                    #endregion
                    var interfaceType = interfaces.FirstOrDefault(x => x.Name == $"I{type.Name}");
                    if (interfaceType == null)
                    {
                        interfaceType = type;
                    }
                    var serviceDescriptor = new ServiceDescriptor(interfaceType, type, lifetime);
                    if (!services.Contains(serviceDescriptor))
                    {
                        services.TryAdd(serviceDescriptor);
                    }
                }
            }
            return services;
        }
    }
}