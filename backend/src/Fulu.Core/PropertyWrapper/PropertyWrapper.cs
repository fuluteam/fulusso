using System;
using System.Reflection;

namespace Fulu.Core.PropertyWrapper
{
    public static class PropertyWrapper
    {
        public static ISetValue CreateSetter(this PropertyInfo propertyInfo)
        {
            if (propertyInfo == null)
                throw new ArgumentNullException("propertyInfo");
            if (propertyInfo.CanWrite == false)
                throw new NotSupportedException("属性不支持写操作。");
            MethodInfo mi = propertyInfo.GetSetMethod(true);
            if (mi.GetParameters().Length > 1)
                throw new NotSupportedException("不支持构造索引器属性的委托。");

            Type instanceType = typeof(SetterPropertyWrapper<,>).MakeGenericType(propertyInfo.DeclaringType, propertyInfo.PropertyType);
            return (ISetValue)Activator.CreateInstance(instanceType, propertyInfo);
        }
        public static IGetValue CreateGetter(this PropertyInfo propertyInfo)
        {
            if (propertyInfo == null)
                throw new ArgumentNullException("propertyInfo");
            if (propertyInfo.CanRead == false)
                throw new NotSupportedException("属性不支持读操作。");
            MethodInfo mi = propertyInfo.GetGetMethod(true);
            if (mi.GetParameters().Length > 1)
                throw new NotSupportedException("不支持构造索引器属性的委托。");

            Type instanceType = typeof(GetterPropertyWrapper<,>).MakeGenericType(propertyInfo.DeclaringType, propertyInfo.PropertyType);
            return (IGetValue)Activator.CreateInstance(instanceType, propertyInfo);
        }
    }
}