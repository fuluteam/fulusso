using System.Threading.Tasks;
using Fulu.Core.Extensions;
using FuLu.Passport.Domain.Interface.Services;
using FuLu.Passport.Domain.Models;
using IdentityServer4.Models;
using Microsoft.AspNetCore.Http;

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
            if (result.Code == 0)
            {
                var claims = await _userService.SaveSuccessLoginInfo(context.Request.ClientId.ToInt32(), result.Data.Id,
                    _contextAccessor.HttpContext.GetIp(), UserLoginModel.Password);
                context.Result = new GrantValidationResult(context.UserName, "custom", claims);
            }
            else
            {
                context.Result = new GrantValidationResult(TokenRequestErrors.InvalidRequest, result.Message);
            }
        }
    }
}
