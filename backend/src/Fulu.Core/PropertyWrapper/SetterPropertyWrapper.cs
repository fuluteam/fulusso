using System;
using System.Reflection;

namespace Fulu.Core.PropertyWrapper
{
    public class SetterPropertyWrapper<TTarget, TValue> : ISetValue
    {
        private Action<TTarget, TValue> _setter;

        public SetterPropertyWrapper(PropertyInfo propertyInfo)
        {
            if (propertyInfo == null)
                throw new ArgumentNullException("propertyInfo");
            if (!propertyInfo.CanWrite)
            {
                throw new NotSupportedException("属性不支持写操作。");
            }

            MethodInfo mi = propertyInfo.GetSetMethod(true);
            _setter = (Action<TTarget, TValue>)Delegate.CreateDelegate(typeof(Action<TTarget, TValue>), null, mi);
        }

        public void Set(object target, object val)
        {
            var type = typeof(TValue);
            if (type.Name == "Nullable`1")
            {
                type = type.GetGenericArguments()[0];
            }
            _setter((TTarget)target, (TValue)Convert.ChangeType(val, type));
        }
    }
}