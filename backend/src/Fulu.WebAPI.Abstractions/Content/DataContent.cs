namespace Fulu.WebAPI.Abstractions
{
    public class DataContent : MethodResult
    {
        public DataContent()
        {
            Code = "0";
            Message = "ok";
        }
        public DataContent(string code, string message)
        {
            Code = code;
            Message = message;
        }

        public object Data { get; set; }

        public override object ExecuteResult()
        {
            return new
            {
                Data = Data,
                Code = Code,
                Message = Message,
                MessageType = MessageType
            };
        }
    }

    public class DataContent<T> : MethodResult
    {
        public DataContent()
        {
            Code = "0";
            Message = "ok";
        }
        public DataContent(string code, string message)
        {
            Code = code;
            Message = message;
        }

        public DataContent(T data, string code = "0", string message = "ok", MessageType messageType = MessageType.Success)
        {
            Code = code;
            Message = message;
            Data = data;
            MessageType = messageType;
        }

        public T Data { get; set; }

        public override object ExecuteResult()
        {
            return new
            {
                Data = Data,
                Code = Code,
                Message = Message,
                MessageType = MessageType
            };
        }
    }
}
