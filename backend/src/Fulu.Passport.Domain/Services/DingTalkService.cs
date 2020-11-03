using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using Fulu.BouncyCastle;
using Fulu.Passport.Domain.Interface;
using Fulu.Passport.Domain.Models;
using Newtonsoft.Json;

namespace Fulu.Passport.Domain.Services
{
    public class DingTalkService : IExternalService
    {
        private HttpClient _client;
        public DingTalkService(HttpClient client)
        {
            _client = client;
        }
        public async Task<string> GetOpenId(string code, string appid, string secret)
        {
            var timestamp = TimeStamp();
            var signature = HMACSHA256.Compute(timestamp, secret);
            var requestUri =
                $"https://oapi.dingtalk.com/sns/getuserinfo_bycode?accessKey={appid}&timestamp={timestamp}&signature={HttpUtility.UrlEncode(signature)}";
            var request = new HttpRequestMessage(HttpMethod.Post, requestUri);
            request.Content = new StringContent(JsonConvert.SerializeObject(new { tmp_auth_code = code }), Encoding.UTF8, "application/json");
            var tokenResponse = await _client.SendAsync(request);
            tokenResponse.EnsureSuccessStatusCode();
            var text = await tokenResponse.Content.ReadAsStringAsync();
            var result = JsonConvert.DeserializeObject<DingTalkUserResponse>(text);
            if (result.Errcode == 0)
            {
                return result.UserInfo.Openid;
            }
            throw new Exception($"获取用户钉钉信息出错，{result.Errmsg}");
        }
        private string TimeStamp()
        {
            var dtStart = new DateTime(1970, 1, 1).ToLocalTime();
            return ((DateTime.Now.Ticks - dtStart.Ticks) / 10000).ToString();
        }
    }
}
