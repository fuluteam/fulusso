using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Reflection;
using System.Text;

namespace Fulu.Core.Extensions
{
    public static class EnumExtensions
    {
        /// <summary>
        /// 返回枚举项的描述信息。
        /// </summary>
        /// <param name="value">要获取描述信息的枚举项。</param>
        /// <returns>枚举想的描述信息。</returns>
        public static string GetDescription(this Enum value)
        {
            var enumType = value.GetType();
            // 获取枚举常数名称。
            var name = Enum.GetName(enumType, value);
            if (name == null) return null;
            // 获取枚举字段。
            var fieldInfo = enumType.GetField(name);
            if (fieldInfo == null) return null;
            // 获取描述的属性。
            if (Attribute.GetCustomAttribute(fieldInfo,
                typeof(DescriptionAttribute), false) is DescriptionAttribute attr)
            {
                return attr.Description;
            }
            return null;
        }

    }
}
