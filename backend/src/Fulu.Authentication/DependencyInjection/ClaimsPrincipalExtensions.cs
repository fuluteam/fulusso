using System.Security.Claims;

namespace IdentityModel
{
    public static partial class ClaimsPrincipalExtensions
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="user"></param>
        /// <returns></returns>
        public static string GetUserId(this ClaimsPrincipal user)
        {
            return user.FindFirst(c => c.Type == JwtClaimTypes.Id)?.Value;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="user"></param>
        /// <returns></returns>
        public static string GetClientId(this ClaimsPrincipal user)
        {
            return user.FindFirst(c => c.Type == JwtClaimTypes.ClientId)?.Value;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="user"></param>
        /// <returns></returns>
        public static string GetNickName(this ClaimsPrincipal user)
        {
            return user.FindFirst(c => c.Type == JwtClaimTypes.NickName)?.Value;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="user"></param>
        /// <returns></returns>
        public static string GetPhoneNo(this ClaimsPrincipal user)
        {
            return user.FindFirst(c => c.Type == JwtClaimTypes.PhoneNumber)?.Value;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="user"></param>
        /// <returns></returns>
        public static string GetOpenId(this ClaimsPrincipal user)
        {
            return user.FindFirst(c => c.Type == "open_id")?.Value;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="user"></param>
        /// <returns></returns>
        public static string GetAuthenticationStatus(this ClaimsPrincipal user)
        {
            return user.FindFirst(c => c.Type == "authentication_status")?.Value;
        }
    }
}
