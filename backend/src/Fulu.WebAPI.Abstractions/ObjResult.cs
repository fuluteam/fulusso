using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Fulu.WebAPI.Abstractions
{
    public class ObjResult : MethodResult
    {
        public ObjResult()
        {
            Code = "0";
            Message = "ok";
        }
        public ObjResult(string code, string message)
        {
            Code = code;
            Message = message;
        }

        public ObjResult(object data, string code = "0", string message = "ok", MessageType messageType = MessageType.Success)
        {
            Code = code;
            Message = message;
            Data = data;
            MessageType = messageType;

            if (data is ICollection list)
            {
                Statistics = new Statistic() { Total = list.Count };
            }
        }

        public string Code { get; set; }

        public string Message { get; set; }

        public MessageType MessageType { get; set; }

        public object Data { get; set; }

        public Statistic Statistics { get; set; }

        public override object ExecuteResult()
        {
            if (Statistics == null)
            {
                return new
                {
                    Data = Data,
                    Code = Code,
                    Message = Message,
                    MessageType = MessageType
                };
            }
            else
            {
                return new
                {
                    Data = Data,
                    Statistics = Statistics,
                    Code = Code,
                    Message = Message,
                    MessageType = MessageType
                };
            }
        }
    }

    public class ObjResult<T> : MethodResult
    {
        public ObjResult(T data, string code = "0", string message = "ok", MessageType messageType = MessageType.Success)
        {
            Code = code;
            Message = message;
            Data = data;
            MessageType = messageType;
        }

        public ObjResult(IEnumerable<T> data, string code = "0", string message = "ok", MessageType messageType = MessageType.Success)
        {
            Code = code;
            Message = message;
            Data = data;
            Statistics = new Statistic() { Total = data.Count() };
            MessageType = messageType;
        }

        public object Data { get; set; }

        public Statistic Statistics { get; set; }

        public override object ExecuteResult()
        {
            if (Statistics == null)
            {
                return new
                {
                    Data = Data,
                    Code = Code,
                    Message = Message,
                    MessageType = MessageType
                };
            }
            else
            {
                return new
                {
                    Data = Data,
                    Statistics = Statistics,
                    Code = Code,
                    Message = Message,
                    MessageType = MessageType
                };
            }
        }
    }

    public class ObjResult<T, T2> : MethodResult where T2 : Statistic, new()
    {
        public ObjResult(IEnumerable<T> data, T2 statistic, string code = "0", string message = "ok", MessageType messageType = MessageType.Success)
        {
            Code = code;
            Message = message;
            Data = data;
            Statistics = statistic;
            MessageType = messageType;
        }

        public object Data { get; set; }

        public Statistic Statistics { get; set; }

        public override object ExecuteResult()
        {
            return new
            {
                Data = Data,
                Statistics = Statistics,
                Code = Code,
                Message = Message,
                MessageType = MessageType
            };
        }
    }
}
