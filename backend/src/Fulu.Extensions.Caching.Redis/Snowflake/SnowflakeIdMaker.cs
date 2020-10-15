using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using StackExchange.Redis;

namespace Microsoft.Extensions.Caching.Redis.Snowflake
{
    public class SnowflakeIdMaker : ISnowflakeIdMaker
    {
        private readonly SnowflakeOption _option;

        private static readonly object _locker = new object();
        //最后的时间戳
        private long _lastTimestamp = -1L;
        //最后的序号
        private int _lastIndex = -1;
        /// <summary>
        /// 当前工作节点
        /// </summary>
        private readonly string _currentWorkIndex;
        /// <summary>
        /// 正在使用的列表
        /// </summary>
        private readonly string _inUse;
        private uint _workId = 0;
        private readonly IServiceProvider _provider;
        public SnowflakeIdMaker(IOptions<SnowflakeOption> options, IServiceProvider provider)
        {
            _provider = provider;
            _option = options.Value;
            _currentWorkIndex = $"current.work.index:{_option.InstanceName}";
            _inUse = $"in.use:{_option.InstanceName}";
            Init().Wait();
        }

        private async Task Init()
        {
            if (_option.EnableAutoWorkId)
            {
                var cache = _provider.GetService<IRedisCache>();
                if (cache == null)
                {
                    throw new Exception("未配置Redis组件。当EnableAutoWorkId值为true时，请务必配置Redis组件");
                }
               
                _workId = (uint)await cache.IncrementAsync(_currentWorkIndex) - 1;
                if (_workId > 1023)
                {
                    //表示所有节点已全部被使用过，则从历史列表中，获取当前已回收的节点id

                    var newWorkdId = await cache.SortedSetRangeByScoreWithScoresAsync(_inUse, 0,
                        DateTime.Now.AddMinutes(5).ToUnixTimestamp(), 0, 1, Order.Ascending);

                    if (!newWorkdId.Any())
                    {
                        throw new Exception("没有可用的节点");
                    }
                    _workId = uint.Parse(newWorkdId.First().Key);
                }
                //将正在使用的workId写入到有序列表中
                await cache.SortedSetAddAsync(_inUse, _workId.ToString(), DateTime.Now.ToUnixTimestamp());

            }
        }

        public long NextId(uint? workId = null)
        {
            if (workId != null)
            {
                _workId = workId.Value;
            }
            if (_workId > 1023)
            {
                throw new Exception("机器码取值范围为0-1023");
            }

            lock (_locker)
            {
                var currentTimeStamp = TimeStamp();
                if (currentTimeStamp < _lastTimestamp)
                {
                    //throw new Exception("时间戳生成出现错误");
                    //发生时钟回拨，切换workId，可解决。
                    Init().Wait();
                    return NextId();
                }
                if (currentTimeStamp == _lastTimestamp)
                {
                    if (_lastIndex < 4095)//为了保证长度
                    {
                        _lastIndex++;
                    }
                    else
                    {
                        throw new Exception("单位毫秒内生成的id超过所支持的数量");
                    }
                }
                else
                {
                    _lastIndex = 0;
                    _lastTimestamp = currentTimeStamp;
                }
                var timeStr = Convert.ToString(currentTimeStamp, 2);
                var workStr = Convert.ToString(_workId, 2).PadLeft(10, '0');
                var indexStr = Convert.ToString(_lastIndex, 2).PadLeft(12, '0');
                return Convert.ToInt64($"0{timeStr}{workStr}{indexStr}", 2);
            }
        }

        public uint WorkId()
        {
            return _workId;
        }

        public string GetUserInKey()
        {
            return _inUse;
        }

        private long TimeStamp()
        {
            var dt1970 = new DateTime(1970, 1, 1);
            return (DateTime.Now.Ticks - dt1970.Ticks) / 10000;
        }
    }
}