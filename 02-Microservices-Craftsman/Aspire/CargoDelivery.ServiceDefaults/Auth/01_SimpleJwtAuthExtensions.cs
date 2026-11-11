// ====================================================================================
// 🔑 МОДУЛЬ 03.1: ПРОСТИЙ CUSTOM JWT (AuthorizationModuleAPI)
// ------------------------------------------------------------------------------------
// Відповідає за реєстрацію схеми перевірки JWT-токенів, випущених сервісом
// AuthorizationModuleAPI за допомогою симетричного ключа (HMAC-SHA256).
//
// КЛЮЧОВІ ХАРАКТЕРИСТИКИ:
//   - Не має OIDC Discovery (/.well-known/openid-configuration).
//   - Валідується локально через спільний секрет (Auth:SimpleJwt:Secret).
//   - У Swagger UI відображається як класичне поле введення Bearer-токена.
// ====================================================================================

using System.Text;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace CargoDelivery.ServiceDefaults.Auth;

public static class SimpleJwtAuthExtensions
{
    public const string DefaultSecret = "TOPSECRETKEYFORDRIVERMANAGEMENTSYSTEM";
    public const string DefaultIssuer = "AuthorizationModuleAPI";
    public const string DefaultAudience = "cargodelivery.microservices";

    public static AuthenticationBuilder AddSimpleJwtScheme(
        this AuthenticationBuilder builder,
        IConfiguration configuration)
    {
        var section = configuration.GetSection("Auth:SimpleJwt");
        var secret = section["Secret"] ?? DefaultSecret;
        var issuer = section["Issuer"] ?? DefaultIssuer;
        var audience = section["Audience"] ?? DefaultAudience;

        return builder.AddJwtBearer(AuthConstants.Schemes.SimpleJwt, options =>
        {
            options.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidIssuer = issuer,
                ValidateAudience = true,
                ValidAudience = audience,
                ValidateLifetime = true,
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(secret)),
                ClockSkew = TimeSpan.FromMinutes(1)
            };
        });
    }

    public static void AddSimpleJwtSwaggerDefinition(this SwaggerGenOptions config)
    {
        config.AddSecurityDefinition(AuthConstants.SwaggerSecurityIds.SimpleBearer, new OpenApiSecurityScheme
        {
            Name = "Authorization",
            Type = SecuritySchemeType.Http,
            Scheme = "bearer",
            BearerFormat = "JWT",
            In = ParameterLocation.Header,
            Description = "【03.1 Simple JWT】 Вставте access_token, отриманий у AuthorizationModuleAPI (POST /accounts/authenticate)."
        });
    }
}
