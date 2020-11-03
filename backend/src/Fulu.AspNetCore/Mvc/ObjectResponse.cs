using Fulu.WebAPI.Abstractions;
using System;
using System.Collections.Generic;
using System.Text;

namespace Microsoft.AspNetCore.Mvc
{
    public class ObjectResponse
    {
        [NonAction]
        public static OkObjectResult Error(string message)
        {
            return new OkObjectResult(ActionObject.Error( message));
        }

        [NonAction]
        public static OkObjectResult Ok(string message)
        {
            return new OkObjectResult(ActionObject.Ok(0, message));
        }

        [NonAction]
        public static OkObjectResult Ok(int code = 0, string message = "ok")
        {
            return new OkObjectResult(ActionObject.Ok(code, message));
        }

        [NonAction]
        public static OkObjectResult Ok<T>(T data, int code = 0, string message = "ok", MessageType messageType = MessageType.Success)
        {
            return data.GetType().Name == typeof(ActionObjectResult<T>).Name ? new OkObjectResult(data) : new OkObjectResult(ActionObject.Ok(data, code, message, messageType));
        }

        [NonAction]
        public static OkObjectResult Ok<T>(T data, Pagination pagination, int code = 0, string message = "ok", MessageType messageType = MessageType.Success)
        {
            return new OkObjectResult(ActionObject.Ok(data, pagination, code, message, messageType));
        }
    }

}
