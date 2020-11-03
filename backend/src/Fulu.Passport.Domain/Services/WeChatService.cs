using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Fulu.Passport.Domain.Interface;
using Fulu.Passport.Domain.Models;
using Newtonsoft.Json;

namespace Fulu.Passport.Domain.Services
{
    public class WeChatService : IExternalService
    {
        private readonly HttpClient _client;

        public WeChatService(HttpClient client)
        {
            _client = client;
        }

        public async Task<string> GetOpenId(string code, string appid, string secret)
        {
            var requestUri =
                $"https://api.weixin.qq.com/sns/oauth2/access_token?appid={appid}&secret={secret}&code={code}&grant_type=authorization_code";
            var request = new HttpRequestMessage(HttpMethod.Get, requestUri);
            var tokenResponse = await _client.SendAsync(request);
            tokenResponse.EnsureSuccessStatusCode();
            var text = await tokenResponse.Content.ReadAsStringAsync();
            var result = JsonConvert.DeserializeObject<WeChatUserResponse>(text);
            if (result.Errcode == 0)
            {
                return result.OpenId;
            }
            throw new Exception($"获取用户微信信息出错，{result.Errmsg}");
        }
    }
}
