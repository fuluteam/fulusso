using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using IdentityServer4.Models;
using FuLu.Passport.Domain.Interface.Services;
using FuLu.Passport.Domain.Models;

namespace IdentityServer4.Validation
{
    public class ResourceOwnerPasswordValidator : IResourceOwnerPasswordValidator
    {
        private readonly IUserService _userService;
        private readonly IHttpContextAccessor _contextAccessor;
        public ResourceOwnerPasswordValidator(IUserService userService, IHttpContextAccessor contextAccessor)
        {
            _userService = userService;
            _contextAccessor = contextAccessor;
        }

        public async Task ValidateAsync(ResourceOwnerPasswordValidationContext context)
        {
            var result = await _userService.LoginByPasswordAsync(context.UserName, context.Password);
            if (result.Code == "0")
            {
                var clientId = int.Parse(context.Request.ClientId);
                var claims = await _userService.GetLoginClaims(result.Data, clientId, _contextAccessor.HttpContext.GetIp(), UserLoginModel.Interface);
                context.Result = new GrantValidationResult(context.UserName, "custom", claims);
            }
            else
            {
                context.Result = new GrantValidationResult(TokenRequestErrors.InvalidRequest, result.Message);
            }
        }
    }
}
