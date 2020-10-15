using Swashbuckle.AspNetCore.SwaggerGen;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.OpenApi.Models;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class SwaggerGenOptionsExtension
    {
        public static void IncludeXmlComments(this SwaggerGenOptions options)
        {
            string[] xmlFiles = Directory.GetFiles(AppContext.BaseDirectory, "*.xml");
            foreach (var file in xmlFiles)
            {
                options.IncludeXmlComments(file, true);
            }
        }

        public static void AddOAuth2Definition(this SwaggerGenOptions options)
        {
            options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
            {
                Description = "JWT授权(数据将在请求头中进行传输) 在下方输入Bearer {token} 即可，注意两者之间有空格",
                Name = "Authorization",
                In = ParameterLocation.Header,
                Type = SecuritySchemeType.ApiKey, //jwt默认的参数名称
            });
        }
    }
}
