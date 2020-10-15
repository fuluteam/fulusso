using System.Linq;
using Microsoft.AspNetCore.Mvc;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class ApiBehaviorOptionsExtensions
    {
        public static IServiceCollection ConfigureApiBehavior(this IServiceCollection services)
        {
            services.Configure<ApiBehaviorOptions>(options =>
            {
                options.InvalidModelStateResponseFactory = actionContext =>
                {
                    var ms = actionContext.ModelState
                        .First(e => e.Value.Errors.Count > 0);

                    var firstError = ms.Value.Errors.First();
                    string message = firstError.Exception != null ? firstError.Exception.Message : firstError.ErrorMessage;
                    var code = "ModelInvalid";
                    var errRes = new { code, message };
                    return new BadRequestObjectResult(errRes);
                };
            });

            return services;
        }
    }
}
