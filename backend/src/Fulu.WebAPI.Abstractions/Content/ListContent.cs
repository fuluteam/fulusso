using System.Collections.Generic;

namespace Fulu.WebAPI.Abstractions
{
    public class ListContent<T> : MethodResult
    {
        public ListContent()
        {
            Data = new List<T>();
            Statistics = new Statistic();
        }
        /// <summary>
        /// 
        /// </summary>
        public List<T> Data { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public Statistic Statistics { get; set; }

        public ListContent(List<T> data, string code = "0", string message = "ok", MessageType messageType = MessageType.Success)
        {
            Code = code;
            Message = message;
            Data = data;
            Statistics = new Statistic { Total = data.Count };
            MessageType = messageType;
        }
    }
}
