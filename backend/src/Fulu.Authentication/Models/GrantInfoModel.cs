namespace Fulu.Authentication.Models
{
    public class GrantInfoModel
    {
        public string ClientId { get; set; }

        public string ClientIdTo { get; set; }

        public bool Granted { get; set; }

        public string Message { get; set; }
    }
}
