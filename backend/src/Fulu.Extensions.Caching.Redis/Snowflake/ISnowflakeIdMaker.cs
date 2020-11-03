namespace Microsoft.Extensions.Caching.Redis.Snowflake
{
    public interface ISnowflakeIdMaker
    {
        long NextId(uint? workId = null);
        uint WorkId();

        string GetUserInKey();
    }
}