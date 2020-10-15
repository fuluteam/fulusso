using Fulu.WebAPI.Abstractions;
using System;
using System.Collections.Generic;
using System.Text;

namespace Microsoft.AspNetCore.Mvc
{
    public class BaseController : ControllerBase
    {
        [NonAction]
        protected virtual OkObjectResult Ok(IMethodResult method)
        {
            return new OkObjectResult(method.ExecuteResult());
        }

        [NonAction]
        protected static OkObjectResult Ok(string code, string message)
        {
            return new OkObjectResult(new DataContent(code, message));
        }

        [NonAction]
        protected static OkObjectResult Ok(object data, string code = "0", string message = "ok", MessageType messageType = MessageType.Success)
        {
            return new OkObjectResult(new DataContent<object>(data, code, message, messageType));
        }

        [NonAction]
        protected static OkObjectResult Ok(List<object> data, Pagination pagination, string code = "0", string message = "ok", MessageType messageType = MessageType.Success)
        {
            return new OkObjectResult(new PageContent<object>(data, pagination, code, message, messageType));
        }
    }

}
