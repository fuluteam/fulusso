using Fulu.Authentication;
using Fulu.Authentication.Models;
using Fulu.Http;
using Fulu.Passport.Domain.Interface;
using Fulu.Passport.Domain.Options;
using FuLu.Passport.Domain.Options;
using IdentityModel.Client;
using Microsoft.Extensions.Caching.Redis;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using IdentityModel;

namespace Fulu.Passport.Domain
{
    public class PassportClient : HttpClientBase, IPassportClient
    {
        private readonly AppSettings _appSettings;
        private readonly IRedisCache _redisCache;
        private readonly IAuthorizeTokenClient _authorizeTokenClient;
        private readonly SmsOptions _smsServerOptions;
        public PassportClient(AppSettings appSettings, IRedisCache redisCache, HttpClient httpClient, IAuthorizeTokenClient authorizeTokenClient, SmsOptions smsServerOptions) : base(httpClient)
        {
            _appSettings = appSettings;
            _redisCache = redisCache;
            _authorizeTokenClient = authorizeTokenClient;
            _smsServerOptions = smsServerOptions;
        }
        private async Task<(string, JwtToken)> GetCacheToken()
        {
            var key = $"client_credentials:{_appSettings.ClientId}";

            var tokenResult = await _redisCache.GetAsync<JwtToken>(key);
            if (!string.IsNullOrWhiteSpace(tokenResult?.AccessToken))
                return ("", tokenResult);

            var (tokenerror, result) = await _authorizeTokenClient.GetToken();

            if (!string.IsNullOrEmpty(tokenerror))
                return (tokenerror, null);

            await _redisCache.AddAsync(key, result, TimeSpan.FromSeconds(int.Parse(result.ExpiresIn) - 300));
            return ("", result);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="receiveNumber"></param>
        /// <param name="messageContent"></param>
        /// <param name="signatureCode"></param>
        /// <returns></returns>
        public async Task<(string code, string msg, string msgId)> SendSms(string receiveNumber, string messageContent, string signatureCode)
        {
            var (error, result) = await GetCacheToken();
            Client.SetBearerToken(result.AccessToken);
            using (var content = new StringContent(JsonConvert.SerializeObject(new { phoneNumbers = receiveNumber, message = messageContent, signatureCoede = signatureCode }), Encoding.UTF8, "application/json"))
            using (var response = await Client.PostAsync(_smsServerOptions.Url, content))
            {
                var text = await response.Content.ReadAsStringAsync();
                var oauth = JsonConvert.DeserializeObject<JObject>(text);
                var code = oauth["code"].Value<string>();
                var msg = oauth["message"].Value<string>();
                var msgId = oauth["data"].Value<string>();
                return (code, msg, msgId);
            }
        }
    }
}
