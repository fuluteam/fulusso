using Microsoft.Extensions.DependencyModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.Loader;
using System.Text;

namespace Fulu.AutoDI
{
    public static class TypeExtensions
    {

        public static List<Assembly> GetCurrentPathAssembly()
        {
            var dlls = DependencyContext.Default.CompileLibraries
                .Where(x => !x.Name.StartsWith("Microsoft") &&
                            !x.Name.StartsWith("System") &&
                            !x.Name.StartsWith("runtime.") &&
                            !x.Name.StartsWith("Newtonsoft") &&
                            (x.Type == "project" || x.Type == "package"))
                .ToList();
            var list = new List<Assembly>();
            if (dlls.Any())
            {
                foreach (var dll in dlls)
                {
                    if (dll.Type == "project")
                    {
                        list.Add(Assembly.Load(dll.Name));
                    }
                    else
                    {
                        var path = dll.ResolveReferencePaths();
                        if (path.Any())
                        {
                            var ass = AssemblyLoadContext.Default.LoadFromAssemblyPath(path.First());
                            list.Add(ass);
                        }
                    }
                }
            }
            return list;
        }

        /// <summary>
        /// 判断指定的类型 <paramref name="type"/> 是否是指定泛型类型的子类型，或实现了指定泛型接口。
        /// </summary>
        /// <param name="type"></param>
        /// <param name="generic"></param>
        /// <returns></returns>
        public static bool HasImplementedRawGeneric(this Type type, Type generic)
        {
            if (type == null) throw new ArgumentNullException(nameof(type));
            if (generic == null) throw new ArgumentNullException(nameof(generic));

            // 测试接口。
            var isTheRawGenericType = type.GetInterfaces().Any(IsTheRawGenericType);
            if (isTheRawGenericType) return true;

            // 测试类型。
            while (type != null && type != typeof(object))
            {
                isTheRawGenericType = IsTheRawGenericType(type);
                if (isTheRawGenericType) return true;
                type = type.BaseType;
            }
            // 没有找到任何匹配的接口或类型。
            return false;
            // 测试某个类型是否是指定的原始接口。
            bool IsTheRawGenericType(Type test)
                => generic == (test.IsGenericType ? test.GetGenericTypeDefinition() : test);
        }
    }
}
