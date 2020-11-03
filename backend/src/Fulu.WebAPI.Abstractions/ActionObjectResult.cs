using System;
using System.Collections.Generic;
using System.Text;

namespace Fulu.WebAPI.Abstractions
{
    public class ActionObjectResult
    {
        public int Code { get; set; }

        public string Message { get; set; }

        public object Data { get; set; }

        public MessageType MessageType { get; set; }

    }

    public class ActionObjectResult<T>
    {
        public int Code { get; set; }

        public string Message { get; set; }

        public T Data { get; set; }

        public MessageType MessageType { get; set; }
    }

    public class ActionObjectResult<T, T2>
    {
        public int Code { get; set; }

        public string Message { get; set; }

        public T Data { get; set; }

        public T2 Statistics { get; set; }

        public MessageType MessageType { get; set; }
    }
}
