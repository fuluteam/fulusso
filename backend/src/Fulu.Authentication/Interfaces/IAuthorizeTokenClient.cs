using System.Threading.Tasks;
using Fulu.Authentication.Models;
using Fulu.WebAPI.Abstractions;

namespace Fulu.Authentication
{
    public interface IAuthorizeTokenClient
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="method"></param>
        /// <param name="authorization"></param>
        /// <returns></returns>
        Task<DataContent<GrantInfoModel>> GetGrantInfo(string method, string authorization);

        /// <summary>
        /// 授权码模式（前端收到授权码，附上早先的"重定向URI"，向通行证服务器申请令牌（由后端服务器转发组装请求，对用户不可见））。
        /// </summary>
        /// <param name="code">表示授权码，必选项。该码的有效期应该很短，通常设为10分钟，客户端只能使用该码一次，否则会被授权服务器拒绝。该码与客户端ID和重定向URI，是一一对应关系。</param>
        /// <param name="state">表示客户端的当前状态，可以指定任意值，认证服务器会原封不动地返回这个值</param>
        /// <param name="redirectUri">表示重定向URI，必选项</param>
        /// <returns></returns>
        Task<(string error, JwtToken result)> GetToken(string code, string state, string redirectUri);
        /// <summary>
        /// 客户端模式（Client Credentials Grant）
        /// </summary>
        /// <returns></returns>
        Task<(string error, JwtToken result)> GetToken();
        /// <summary>
        /// 密码模式（Resource Owner Password Credentials Grant）
        /// </summary>
        /// <param name="username">表示用户名，必选项。</param>
        /// <param name="password">表示用户的密码，必选项。</param>
        /// <returns></returns>
        Task<(string error, JwtToken result)> GetToken(string username, string password);

    }
}
