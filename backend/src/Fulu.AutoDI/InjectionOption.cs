using Microsoft.Extensions.DependencyInjection;

namespace Fulu.AutoDI
{
    public class InjectionOption
    {
        /// <summary>
        /// 类库名前缀
        /// </summary>
        public string LibPrefix { get; set; }
        /// <summary>
        /// 类全名包含的字符串
        /// </summary>
        public string[] MatchNames { get; set; }

        public ServiceLifetime Lifetime { get; set; } = ServiceLifetime.Scoped;
    }
}