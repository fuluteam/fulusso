using System;
using System.Collections;
using System.Linq;
using System.Text;

namespace Fulu.WebAPI.Abstractions
{
    public class ActionObject
    {
        public static ActionObjectResult Ok(string message)
        {
            return new ActionObjectResult { Data = default, Code = 0, Message = message, MessageType = MessageType.Success };
        }
        public static ActionObjectResult Ok(int code = 0, string message = "ok")
        {
            return new ActionObjectResult { Data = default, Code = code, Message = message, MessageType = MessageType.Success };
        }

        public static ActionObjectResult<T> Ok<T>(int code = 0, string message = "ok", MessageType messageType = MessageType.Success)
        {
            return new ActionCollectionResult<T>
            {
                Data = default,
                Statistics = new Statistic { Total = 1 },
                Code = code,
                Message = message,
                MessageType = messageType
            };
        }

        public static ActionObjectResult<T> Ok<T>(T data, int code = 0, string message = "ok", MessageType messageType = MessageType.Success)
        {
            if (data is ICollection list)
            {
                return new ActionCollectionResult<T>
                {
                    Data = data,
                    Statistics = new Statistic { Total = list.Count },
                    Code = code,
                    Message = message,
                    MessageType = messageType
                };
            }
            return new ActionObjectResult<T> { Code = code, Data = data, Message = message, MessageType = messageType };
        }

        public static ActionObjectResult<T, T2> Ok<T, T2>(T data, T2 statistics, int code = 0, string message = "ok", MessageType messageType = MessageType.Success) where T2 : Statistic
        {
            if (data is ICollection list)
            {
                statistics.Total = list.Count;
            }
            else
            {
                statistics.Total = 1;
            }

            return new ActionObjectResult<T, T2>
            {
                Data = data,
                Statistics = statistics,
                Code = code,
                Message = message,
                MessageType = messageType
            };
        }

        public static ActionObjectResult<T, Pagination> Ok<T>(T data, Pagination pagination, int code = 0, string message = "ok", MessageType messageType = MessageType.Success)
        {
            if (data is ICollection list)
            {
                pagination.Total = list.Count;
            }
            else
            {
                pagination.Total = 1;
            }

            return new ActionObjectResult<T, Pagination> { Data = data, Statistics = pagination, Code = code, Message = message, MessageType = messageType };
        }

        public static ActionObjectResult Error(string message)
        {
            return new ActionObjectResult { Data = default, Code = -1, Message = message, MessageType = MessageType.Error };
        }
    }
}
