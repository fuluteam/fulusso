using System;
using System.Collections.Generic;
using System.Text;

namespace Fulu.WebAPI.Abstractions
{
    public abstract class MethodResult : IMethodResult
    {
        public string Code { get; set; }

        public string Message { get; set; }

        public MessageType MessageType { get; set; }

        public virtual object ExecuteResult()
        {
            return new { Code, Message, MessageType };
        }
    }
}
