using System;
namespace Microsoft.Extensions.Logging
{
    // eventId: 11
    public static class ErrorResponseLoggerExtensions
    {
        private static readonly Action<ILogger, string, string, Exception> _errorResponse;

        static ErrorResponseLoggerExtensions()
        {
            _errorResponse = LoggerMessage.Define<string, string>(
                LogLevel.Information, 
                new EventId(),
                "Produce error response: [{code}]{message})");
        }

        public static void ErrorResponseProduced(this ILogger logger, string code, string message)
        {
            _errorResponse(logger, code, message, null);
        }
    }

    public static class ErrorHandlerLoggerExtensions
    {
        // ExceptionHandlerMiddleware & DeveloperExceptionPageMiddleware
        private static readonly Action<ILogger, Exception> _unhandledException =
            LoggerMessage.Define(LogLevel.Error, new EventId(1, "UnhandledException"), "An unhandled exception has occurred while executing the request.");

        // ExceptionHandlerMiddleware
        private static readonly Action<ILogger, Exception> _responseStartedErrorHandler =
            LoggerMessage.Define(LogLevel.Warning, new EventId(2, "ResponseStarted"), "The response has already started, the error handler will not be executed.");

        public static void UnhandledException(this ILogger logger, Exception exception)
        {
            _unhandledException(logger, exception);
        }

        public static void ResponseStartedErrorHandler(this ILogger logger)
        {
            _responseStartedErrorHandler(logger, null);
        }
    }
}
