using Newtonsoft.Json;

namespace FuLu.Passport.Domain.Models
{
    /// <summary>
    /// 
    /// </summary>
    public class AddressInfo
    {
        /// <summary>
        /// 
        /// </summary>
        public string Nation { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string Province { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string City { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string District { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string Adcode { get; set; }
    }
    /// <summary>
    /// 
    /// </summary>
    public class IpResult
    {
        /// <summary>
        /// 
        /// </summary>
        [JsonProperty("ad_info")]
        public AddressInfo AddressInfo { get; set; }
    }
}