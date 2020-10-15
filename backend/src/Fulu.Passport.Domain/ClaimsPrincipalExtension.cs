using System.Linq;
using System.Security.Claims;

namespace IdentityModel
{
    public static partial class ClaimsPrincipalExtension
    {
        public static string GetLoginIp(this ClaimsPrincipal user)
        {
            return user.GetValue(CusClaimTypes.LoginIp);
        }
        public static string GetLoginAddress(this ClaimsPrincipal user)
        {
            return user.GetValue(CusClaimTypes.LoginAddress);
        }
        public static string GetLastLoginIp(this ClaimsPrincipal user)
        {
            return user.GetValue(CusClaimTypes.LastLoginIp);
        }
        public static string GetLastLoginAddress(this ClaimsPrincipal user)
        {
            return user.GetValue(CusClaimTypes.LastLoginAddress);
        }
        public static string GetBindingStatus(this ClaimsPrincipal user)
        {
            return user.GetValue(CusClaimTypes.BindingStatus);
        }
      
        public static string GetPhone(this ClaimsPrincipal user)
        {
            return user.GetValue(JwtClaimTypes.PhoneNumber);
        }
        public static string GetUserName(this ClaimsPrincipal user)
        {
            return user.GetValue(JwtClaimTypes.Name);
        }
        public static string GetValue(this ClaimsPrincipal user, string type)
        {
            return user?.Claims.FirstOrDefault(x => x.Type == type)?.Value;
        }
    }
}