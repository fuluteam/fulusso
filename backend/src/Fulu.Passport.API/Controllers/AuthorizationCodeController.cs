using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Fulu.Authentication;
using Fulu.Authentication.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Fulu.Passport.API.Controllers
{
    /// <summary>
    /// 授权码code换取令牌
    /// </summary>
    [Route("api/authorization_code")]
    [ApiController]
    public class AuthorizationCodeController : BaseController
    {
        private readonly IAuthorizeTokenClient _authorizeClient;
        /// <summary>
        /// 
        /// </summary>
        /// <param name="authorizeClient"></param>
        public AuthorizationCodeController(IAuthorizeTokenClient authorizeClient)
        {
            _authorizeClient = authorizeClient;
        }

        /// <summary>
        /// 根据code得到令牌信息
        /// </summary>
        [HttpPost]
        [AllowAnonymous]
        public async Task<IActionResult> Post([FromBody] AuthorizationCodeModel data)
        {
            var (error, result) = await _authorizeClient.GetToken(data.Code, data.State, data.RedirectUri);
            return !string.IsNullOrEmpty(error) ? Ok("-1", error) : Ok(result);
        }

    }
}
