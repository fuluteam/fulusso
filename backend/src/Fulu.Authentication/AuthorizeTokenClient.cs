using Fulu.Authentication.Models;
using Fulu.Authentication.Options;
using Fulu.Http;
using Fulu.WebAPI.Abstractions;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Polly;
using Polly.Timeout;
using System;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace Fulu.Authentication
{
    /// <summary>
    /// 
    /// </summary>
    public class AuthorizeTokenClient : HttpClientBase, IAuthorizeTokenClient
    {
        private readonly AuthorizeTokenOptions _options;
        public AuthorizeTokenClient(HttpClient client, IOptions<AuthorizeTokenOptions> options) : base(client)
        {
            _options = options?.Value ?? throw new ArgumentNullException(nameof(options));
        }
        /// <summary>
        /// 
        /// </summary>
        public async Task<ActionObjectResult<GrantInfoModel>> GetGrantInfo(string method, string authorization)
        {
            using (var httpRequest = new HttpRequestMessage(HttpMethod.Get, $"/api/apiauth/grantinfo/{_options.ClientId}/{method}"))
            {
                httpRequest.Headers.Add("Authorization", authorization);
                using (var responseMessage = await Client.SendAsync(httpRequest))
                {
                    responseMessage.EnsureSuccessStatusCode();

                    var msg = await responseMessage.Content.ReadAsStringAsync();

                    try
                    {
                        var data = JsonConvert.DeserializeObject<ActionObjectResult<GrantInfoModel>>(msg);
                        return data;
                    }
                    catch (JsonSerializationException ex)
                    {
                        return (ActionObject.Ok<GrantInfoModel>(-1,ex.Message));
                    }
                }
            }
        }

        public Task<(string error, JwtToken result)> GetToken(string code, string state, string redirectUri)
        {
            var body = $"grant_type=authorization_code&code={code}&state={state}&redirect_uri={redirectUri}&client_id={_options.ClientId}&client_secret={_options.ClientSecret}";
            return GetTokenAsync(body);
        }

        public Task<(string error, JwtToken result)> GetToken()
        {
            var body = $"grant_type=client_credentials&client_id={_options.ClientId}&client_secret={_options.ClientSecret}";
            return GetTokenAsync(body);
        }

        public Task<(string error, JwtToken result)> GetToken(string username, string password)
        {
            var body = $"grant_type=password&client_id={_options.ClientId}&client_secret={_options.ClientSecret}&username={username}&password={password}";
            return GetTokenAsync(body);
        }

        async Task<(string error, JwtToken result)> GetTokenAsync(string content)
        {
            using (var responseMessage = await Client.PostAsync("/oauth/token", new StringContent(content, Encoding.UTF8, "application/x-www-form-urlencoded")))
            {
                responseMessage.EnsureSuccessStatusCode();

                var msg = await responseMessage.Content.ReadAsStringAsync();

                var result = JsonConvert.DeserializeObject<JwtToken>(msg);
                return (null, result);
            }
        }

    }
}
