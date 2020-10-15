using Newtonsoft.Json;
using System;
using System.Net.Http;
using System.Text;

namespace Fulu.Http
{
    public class HttpClientBase
    {
        public HttpClientBase(HttpClient client)
        {
            Client = client;
        }
        protected HttpClient Client { get; }

        protected StringContent JsonContent(string json) =>
            new StringContent(json, Encoding.UTF8, "application/json");

        protected StringContent JsonContent(object obj) => JsonContent(JsonConvert.SerializeObject(obj));
    }
}
