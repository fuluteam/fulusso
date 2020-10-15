namespace Fulu.Core.Common
{
    public class Error
    {
        public Error()
        { }

        public Error(string code, string message)
        {
            Code = code;
            Message = message;
        }

        public string Code { get; set; }
        public string Message { get; set; }
    }
}
