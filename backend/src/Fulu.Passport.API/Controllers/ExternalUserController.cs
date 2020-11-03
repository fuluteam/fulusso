using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Fulu.Core.Extensions;
using Fulu.Passport.Domain.Interface.Services;
using Fulu.Passport.Domain.Models;
using Fulu.WebAPI.Abstractions;
using IdentityModel;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Fulu.Passport.API.Controllers
{
    /// <summary>
    /// 第三方用户接口
    /// </summary>
    [Route("api/[controller]/[action]")]
    [ApiController]
    public class ExternalUserController : ControllerBase
    {
        private readonly IMapper _mapper;
        private readonly IExternalUserService _externalUserService;
        public ExternalUserController(IExternalUserService externalUserService, IMapper mapper)
        {
            _externalUserService = externalUserService;
            _mapper = mapper;
        }
        /// <summary>
        /// （♂）获取用户绑定列表
        /// </summary>
        [HttpGet]
        [Authorize(Roles = ClaimRoles.User)]
        [ProducesResponseType(typeof(ActionObjectResult<List<GetExternalUserOutputDto>, Statistic>), 200)]
        public async Task<IActionResult> GetUsers()
        {
            var externalUsers = await _externalUserService.GetExternalUsers(User.GetClientId().ToInt32(), User.GetUserId());

            var getUserOutputs = _mapper.Map<List<GetExternalUserOutputDto>>(externalUsers);
            return ObjectResponse.Ok(getUserOutputs);
        }


        /// <summary>
        /// （★）获取单个用户
        /// </summary>
        [HttpGet]
        [Authorize(Roles = ClaimRoles.Client)]
        [ProducesResponseType(typeof(ActionObjectResult<GetExternalUserOutputDto>), 200)]
        public async Task<IActionResult> GetUser([FromQuery] GetExternalUserInputDto inputDto)
        {
            var externalUser = await _externalUserService.GetExternalUser(User.GetClientId().ToInt32(), inputDto.ProviderKey);
            if (null == externalUser)
            {
                return ObjectResponse.Error("第三方用户不存在");
            }

            var externalUserOutput = _mapper.Map<GetExternalUserOutputDto>(externalUser);

            return ObjectResponse.Ok(externalUserOutput);
        }

        /// <summary>
        /// （♂）绑定用户
        /// </summary>
        [HttpPost]
        [Authorize(Roles = ClaimRoles.User)]
        [ProducesResponseType(typeof(ActionObjectResult), 200)]
        public async Task<IActionResult> BindUser([FromBody] BindExternalUserInputDto inputDto)
        {
            var result = await _externalUserService.BindExternalUser(User.GetClientId().ToInt32(), User.GetUserId(), inputDto.ProviderKey,
                  inputDto.LoginProvider, inputDto.NickName);

            return ObjectResponse.Ok(result);
        }

        /// <summary>
        ///（♂）解绑用户
        /// </summary>
        [HttpPost]
        [Authorize(Roles = ClaimRoles.User)]
        [ProducesResponseType(typeof(ActionObjectResult), 200)]
        public async Task<IActionResult> UnbindUser([FromBody] UnbindExternalUserInputDto inputDto)
        {
            await _externalUserService.UnBindExternalUser(User.GetClientId().ToInt32(), User.GetUserId(),
                inputDto.LoginProvider);

            return ObjectResponse.Ok();
        }
    }
}
