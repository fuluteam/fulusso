using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace Fulu.AspNetCore.Mvc
{
    public static class ControllerExtensions
    {
        /// <summary>
        /// 返回错误响应
        /// </summary>
        /// <param name="controller"></param>
        /// <param name="code">错误码</param>
        /// <param name="message">错误信息</param>
        /// <returns></returns>
        public static BadRequestObjectResult Error(this ControllerBase controller, string code, string message = null)
        {
            return controller.BadRequest(new { code, message });
        }

        /// <summary>
        /// 返回错误响应 并记录日志
        /// </summary>
        /// <param name="controller"></param>
        /// <param name="logger"></param>
        /// <param name="code">错误码</param>
        /// <param name="message">错误日志</param>
        /// <returns></returns>
        public static BadRequestObjectResult Error(this ControllerBase controller, ILogger logger, string code, string message = null)
        {
            logger.ErrorResponseProduced(code, message);
            return controller.BadRequest(new { code, message });
        }

        ///// <summary>
        ///// 返回错误响应
        ///// </summary>
        ///// <param name="controller"></param>
        ///// <param name="error">错误</param>
        ///// <returns></returns>
        //public static BadRequestObjectResult Error(this ControllerBase controller, Error error)
        //{
        //    return controller.BadRequest(error);
        //}

        ///// <summary>
        ///// 返回错误响应 并记录日志
        ///// </summary>
        ///// <param name="controller"></param>
        ///// <param name="logger"></param>
        ///// <param name="error">错误</param>
        ///// <returns></returns>
        //public static BadRequestObjectResult Error(this ControllerBase controller, ILogger logger, Error error)
        //{
        //    logger.ErrorResponseProduced(error.Code, error.Message);
        //    return controller.BadRequest(error);
        //}
    }
}
