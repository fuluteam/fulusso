using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Fulu.Authentication;
using Fulu.Authentication.Models;
using Fulu.BouncyCastle;
using Fulu.Passport.Domain.Options;
using FuLu.Passport.Domain.Interface;
using FuLu.Passport.Domain.Models;
using FuLu.Passport.Domain.Options;
using IdentityModel.Client;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Caching.Redis;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Fulu.Passport.Domain.Services
{
    /// <summary>
    /// 
    /// </summary>
    public class ExternalClient : IExternalClient
    {
        private readonly IRedisCache _redisCache;
        private readonly ILogger<IExternalClient> _logger;
        private readonly IHttpContextAccessor _contextAccessor;
        private readonly AppSettings _appSettings;
        private readonly HttpClient _httpClient;
        private readonly MapOptions _mapOptions;
        private readonly CaptchaOptions _captchaOptions;
        /// <summary>
        /// 
        /// </summary>
        public ExternalClient(ILogger<IExternalClient> logger, IHttpContextAccessor contextAccessor, IRedisCache redisCache, AppSettings appSettings, HttpClient httpClient, MapOptions mapOptions, CaptchaOptions captchaOptions)
        {
            _logger = logger;
            _contextAccessor = contextAccessor;
            _redisCache = redisCache;
            _appSettings = appSettings;
            _httpClient = httpClient;
            _mapOptions = mapOptions;
            _captchaOptions = captchaOptions;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="ip"></param>
        /// <returns></returns>
        public async Task<ResultBase<IpResult>> IpLocation(string ip)
        {
            var querys = new Dictionary<string, string>
            {
                {"key",_mapOptions.Key },
                {"ip",ip }
            };
            var queryString = QueryToString(querys);
            var path = $"{_mapOptions.Endpoint}/ws/location/v1/ip";
            var sig = GenerateGetSig(path, queryString);

            var resData = await _httpClient.GetStringAsync($"{path}?{queryString}&sig={sig}");
            return JsonConvert.DeserializeObject<ResultBase<IpResult>>(resData);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="ip"></param>
        /// <returns></returns>
        public async Task<string> IpToAddress(string ip)
        {
            var key = $"ipinfo:{ip}";
            var address = await _redisCache.StringGetAsync(key);
            if (string.IsNullOrEmpty(address))
            {
                var ipResult = await IpLocation(ip);
                if (ipResult.Status == 0 && ipResult.Result?.AddressInfo != null)
                {
                    var info = ipResult.Result?.AddressInfo;
                    if (string.IsNullOrEmpty(info.Province))
                    {
                        address = await StandbyIpToAddress(ip);
                    }
                    else
                    {
                        address = $"{info.Province}{info.City}";

                    }
                }
                else if (ipResult.Status == 375)
                {
                    address = "本地局域网";
                }
                else
                {
                    address = await StandbyIpToAddress(ip);
                }
                await _redisCache.StringSetAsync(key, address);
            }
            return address;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="ip"></param>
        /// <returns></returns>
        public async Task<string> BdIpToAddress(string ip)
        {
            var key = $"ipinfo:{ip}";
            var address = await _redisCache.StringGetAsync(key);
            if (string.IsNullOrEmpty(address))
            {
                try
                {
                    address = await StandbyIpToAddress(ip);
                }
                catch
                {
                    var ipResult = await IpLocation(ip);
                    if (ipResult.Status == 0)
                    {
                        var addInfo = ipResult.Result?.AddressInfo;
                        if (addInfo != null)
                        {
                            address = $"{addInfo.Province}{addInfo.City}";
                        }
                    }
                }
                if (string.IsNullOrEmpty(address))
                {
                    address = "本地局域网";
                }
                await _redisCache.StringSetAsync(key, address);
            }
            return address;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="ip"></param>
        /// <returns></returns>
        public async Task<string> StandbyIpToAddress(string ip)
        {
            var url = $"https://sp0.baidu.com/8aQDcjqpAAV3otqbppnN2DJv/api.php?query={ip}&resource_id=6006";

            _httpClient.Timeout = TimeSpan.FromMilliseconds(500);
            var resData = await _httpClient.GetStringAsync(url);
            var resjson = JsonConvert.DeserializeObject<JObject>(resData);
            var res = resjson["data"][0]["location"].Value<string>();
            return res.Split(' ')[0];
        }

        private string QueryToString(Dictionary<string, string> queryParams)
        {
            var sorted = queryParams.OrderBy(x => x.Key);
            return string.Join("&", sorted.Select(x => $"{x.Key}={x.Value}"));
        }
        private string GenerateGetSig(string path, string queryString)
        {
            return MD5.Compute($"{path}?{queryString}{_mapOptions.SigKey}");
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="ticket"></param>
        /// <param name="randStr"></param>
        /// <returns></returns>
        public async Task<(int, string)> CaptchaTicketVerify(string ticket, string randStr)
        {
            if (!_appSettings.DeveloperMode)
            {
                var requestUri =
                    $"https://ssl.captcha.qq.com/ticket/verify?aid={_captchaOptions.AppId}&AppSecretKey={_captchaOptions.AppSecretKey}&Ticket={ticket}&Randstr={randStr}&UserIP={_contextAccessor.HttpContext.GetIp()}";
                using (var verifyResponse = await _httpClient.GetAsync(requestUri))
                {
                    verifyResponse.EnsureSuccessStatusCode();
                    var text = await verifyResponse.Content.ReadAsStringAsync();
                    var verifydata = JObject.Parse(text);
                    var response = verifydata["response"].Value<int>();
                    //var evil_level = verifydata["evil_level"].Value<string>();
                    var err_msg = verifydata["err_msg"].Value<string>();
                    return (response, err_msg);
                }
            }

            return (1, "ok");
        }
    }
}
