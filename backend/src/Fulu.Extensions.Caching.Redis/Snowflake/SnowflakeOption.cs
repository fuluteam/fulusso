namespace Microsoft.Extensions.Caching.Redis.Snowflake
{
    public class SnowflakeOption
    {
        /// <summary>
        /// 工作机器ID
        /// </summary>
        public uint WorkId { get; set; }
        /// <summary>
        /// 是否启动自动生成数据中心id和工作机器id，默认为false，为true时，依赖redis组件
        /// </summary>
        public bool EnableAutoWorkId { get; set; }

        /// <summary>
        /// 实例名称。 同一实例下的id唯一。EnableAutoWorkId为true时，此参数有效
        /// </summary>
        public  string InstanceName { get; set; } ="snowflake";
    }
}