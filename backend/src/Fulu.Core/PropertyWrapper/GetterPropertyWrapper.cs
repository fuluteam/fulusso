using System;
using System.Reflection;

namespace Fulu.Core.PropertyWrapper
{
    public class GetterPropertyWrapper<TTarget, TValue>:IGetValue
    {
        private Func<TTarget, TValue> _getter;

        public GetterPropertyWrapper(PropertyInfo propertyInfo)
        {
            if (propertyInfo == null)
                throw new ArgumentNullException("propertyInfo");
            if (!propertyInfo.CanRead)
            {
                throw new NotSupportedException("属性不支持读操作。");
            }

            MethodInfo mi = propertyInfo.GetGetMethod(true);
            _getter = (Func<TTarget, TValue>)Delegate.CreateDelegate(typeof(Func<TTarget, TValue>), null, mi);
        }
        public object Get(object target)
        {
            return _getter((TTarget)target);
        }
    }
}