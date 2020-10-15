using System;
using System.Collections.Generic;
using System.Text;

namespace Fulu.WebAPI.Abstractions
{
    public abstract class ResultBase
    {
        //public virtual MethodResult Ok(string code = "0", string message = "ok")
        //{
        //    return new ObjResult(code, message);
        //}

        //public virtual MethodResult Ok(object data, string code = "0", string message = "ok", MessageType messageType = MessageType.Success)
        //{
        //    return new ObjResult(data, code, message, messageType);
        //}

        //public virtual MethodResult Ok(object data, Pagination pagination, string code = "0", string message = "ok", MessageType messageType = MessageType.Success)
        //{
        //    return new PageResult(data, pagination, code, message, messageType);
        //}

        public static DataContent<object> Ok()
        {
            return new DataContent<object> { Data = new { }, Code = "0", Message = "ok", MessageType = MessageType.Success };
        }

        public static DataContent<object> Ok(string code, string message)
        {
            return new DataContent<object> { Data = new { }, Code = "0", Message = "ok", MessageType = MessageType.Success };
        }

        public static DataContent<T> Ok<T>(T data, string code = "0", string message = "ok", MessageType messageType = MessageType.Success)
        {
            return new DataContent<T> { Code = code, Data = data, Message = message, MessageType = messageType };
        }

        public static ListContent<T> Ok<T>(List<T> list, string code = "0", string message = "ok", MessageType messageType = MessageType.Success)
        {
            return new ListContent<T>
            {
                Data = list,
                Statistics = new Statistic { Total = list.Count },
                Code = code,
                Message = message,
                MessageType = messageType
            };
        }

        public static PageContent<T, T2> Ok<T, T2>(List<T> data, T2 statistics, string code = "0", string message = "ok", MessageType messageType = MessageType.Success) where T2 : Statistic
        {
            return new PageContent<T, T2> { Code = code, Message = message, MessageType = messageType, Data = data, Statistics = statistics };
        }

        public static PageContent<T, Pagination> Ok<T>(List<T> data, Pagination pagination, string code = "0", string message = "ok", MessageType messageType = MessageType.Success)
        {
       
            return new PageContent<T, Pagination> { Data = data, Statistics = pagination, Code = code, Message = message, MessageType = messageType };
        }
    }
}
