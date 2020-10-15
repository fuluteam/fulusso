using System.Collections.Generic;

namespace Fulu.WebAPI.Abstractions
{
    public class PageContent<T> : MethodResult
    {
        public PageContent(List<T> data, Pagination pagination, string code = "0", string message = "ok", MessageType messageType = MessageType.Success)
        {
            Data = data;
            Statistics = pagination;
            Code = code;
            Message = message;
            MessageType = messageType;
        }

        /// <summary>
        /// 
        /// </summary>
        public List<T> Data { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public Pagination Statistics { get; set; }

        public override object ExecuteResult()
        {
            return new { Data = Data, Statistics = Statistics, Code = Code, Message = Message, MessageType = MessageType };
        }
    }

    public class PageContent<T, T2> : BaseContent
    {
        public PageContent()
        {
            
        }
        public PageContent(List<T> data, T2 pagination, string code = "0", string message = "ok", MessageType messageType = MessageType.Success)
        {
            Data = data;
            Statistics = pagination;
            Code = code;
            Message = message;
            MessageType = messageType;
        }
        /// <summary>
        /// 
        /// </summary>
        public List<T> Data { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public T2 Statistics { get; set; }

        public override object ExecuteResult()
        {
            return new { Data = Data, Statistics = Statistics, Code = Code, Message = Message, MessageType = MessageType };
        }
    }
}
