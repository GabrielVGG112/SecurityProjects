using Infrastructure.Presentation;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace VictoriaIdentityProvider.Api.Startup
{
    public static class DefaultConfigurationMiddlewares
    {

        public static void AddCustomControllers(this IServiceCollection services)
        {
            services.AddControllers(o =>
            {
                o.RespectBrowserAcceptHeader = true;
                o.ReturnHttpNotAcceptable = true;
            })
                .AddApplicationPart(typeof(ReferenceToAssembly).Assembly)
                .AddJsonOptions(o =>
  {
      o.JsonSerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
      o.JsonSerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
      o.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
  });
       
        }

        public static void AddJwtMiddleware(this IServiceCollection services, IConfiguration config)
        {
            services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
         .AddJwtBearer(options =>
         {
             options.TokenValidationParameters = new TokenValidationParameters
             {
                 ValidateIssuer = true,
                 ValidateAudience = true,
                 ValidateIssuerSigningKey = true,
                 ValidIssuer = "VictoriaIDP",
                 ValidAudience = "VictoriaAPI",
                 IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(config["Security:Jwt:VictoriaIdpClientSecret"] ?? string.Empty))
             };
         });
        }
    }


}
