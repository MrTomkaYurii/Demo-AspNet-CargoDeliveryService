// ====================================================================================
// 🏛️ МОДУЛЬ 03.2: DUENDE IDENTITYSERVER (OIDC / OAuth 2.0 + PKCE)
// ------------------------------------------------------------------------------------
// Відповідає за реєстрацію схеми перевірки токенів від Duende IdentityServer.
// Також обслуговує сценарій федерації (03.3), коли користувач входить у Duende
// через зовнішній Keycloak, а Duende випускає фінальний токен для мікросервісів.
//
// КЛЮЧОВІ ХАРАКТЕРИСТИКИ:
//   - Використовує OIDC Discovery для автоматичного отримання публічних ключів (JWKS).
//   - Токени мають тип "at+jwt" (RFC 9068) та асиметричний підпис RS256.
//   - У Swagger UI налаштовується Authorization Code Flow + PKCE.
// ====================================================================================

using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace CargoDelivery.ServiceDefaults.Auth;

public static class DuendeAuthExtensions
{
    public static AuthenticationBuilder AddDuendeScheme(
        this AuthenticationBuilder builder,
        IConfiguration configuration,
        IHostEnvironment env)
    {
        var authority = configuration["Auth:Duende:Authority"]
                        ?? configuration["Auth:Authority"]
                        ?? "https://localhost:5001";

        return builder.AddJwtBearer(AuthConstants.Schemes.Duende, options =>
        {
            options.Authority = authority;
            options.RequireHttpsMetadata = !env.IsDevelopment();

            options.TokenValidationParameters.ValidateAudience = false;
            options.TokenValidationParameters.ValidTypes = new[] { "at+jwt", "JWT" };

            if (env.IsDevelopment())
            {
                // У локальному Aspire-стенді дозволяємо self-signed сертифікат розробника для backchannel-запитів JWKS
                options.BackchannelHttpHandler = new HttpClientHandler
                {
                    ServerCertificateCustomValidationCallback = HttpClientHandler.DangerousAcceptAnyServerCertificateValidator
                };
            }
        });
    }

    public static void AddDuendeSwaggerDefinition(this SwaggerGenOptions config, IConfiguration configuration)
    {
        var authority = (configuration["Auth:Duende:Authority"] ?? "https://localhost:5001").TrimEnd('/');

        config.AddSecurityDefinition(AuthConstants.SwaggerSecurityIds.DuendeOAuth2, new OpenApiSecurityScheme
        {
            Type = SecuritySchemeType.OAuth2,
            Description = "【03.2 Duende OIDC】 Authorization Code + PKCE. Підтримує локальних користувачів (alice/bob) та федерацію через Keycloak.",
            Flows = new OpenApiOAuthFlows
            {
                AuthorizationCode = new OpenApiOAuthFlow
                {
                    AuthorizationUrl = new Uri($"{authority}/connect/authorize"),
                    TokenUrl = new Uri($"{authority}/connect/token"),
                    Scopes = new Dictionary<string, string>
                    {
                        { "openid", "OpenID Connect" },
                        { "profile", "Профіль користувача" },
                        { "email", "Email адреса" },
                        { "drivers_management", "Доступ до DriversManagement" },
                        { "cargo_ordering", "Доступ до OrderingManagement" },
                        { "blog_management", "Доступ до BlogManagement" }
                    }
                }
            }
        });
    }
}
