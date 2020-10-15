using System;
using System.Collections.Generic;
using System.Text;

namespace Fulu.WebAPI.Abstractions
{
    public class PageResult<T> : MethodResult
    {
        public PageResult(List<T> data, Pagination pagination, string code = "0", string message = "ok", MessageType messageType = MessageType.Success)
        {
            Data = data;
            Pagination = pagination;
            Code = code;
            Message = message;
            MessageType = messageType;
        }
        public string Code { get; set; }

        public string Message { get; set; }

        public MessageType MessageType { get; set; }

        public List<T> Data { get; set; }

        public Pagination Pagination { get; set; }

        public override object ExecuteResult()
        {
            return new { Data = Data, Statistics = Pagination, Code = Code, Message = Message, MessageType = MessageType };
        }
    }

    public class PageResult<T, T2> : MethodResult where T2 : Statistic, new()
    {
        public PageResult(List<T> data, T2 statistics, string code = "0", string message = "ok", MessageType messageType = MessageType.Success)
        {
            Code = code;
            Statistics = statistics;
            Code = code;
            Message = message;
            MessageType = messageType;
        }
        public string Code { get; set; }

        public string Message { get; set; }

        public MessageType MessageType { get; set; }

        public List<T> Data { get; set; }

        public T2 Statistics { get; set; }

        public override object ExecuteResult()
        {
            return new { Data = Data, Statistics = Statistics, Code = Code, Message = Message, MessageType = MessageType };
        }
    }
}
