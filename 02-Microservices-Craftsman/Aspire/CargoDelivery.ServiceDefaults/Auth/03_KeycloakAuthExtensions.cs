// ====================================================================================
// 🏢 МОДУЛЬ 03.3: ENTERPRISE IAM KEYCLOAK (Realm DevRealmDrivers + Pulumi IaC)
// ------------------------------------------------------------------------------------
// Відповідає за реєстрацію схеми прямої валідації токенів від Keycloak.
// Разом із Duende-федерацією дозволяє продемонструвати як прямий вхід через Keycloak,
// так і брокерську модель (Keycloak -> Duende -> Мікросервіси).
// ====================================================================================

using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace CargoDelivery.ServiceDefaults.Auth;

public static class KeycloakAuthExtensions
{
    public static AuthenticationBuilder AddKeycloakScheme(
        this AuthenticationBuilder builder,
        IConfiguration configuration,
        IHostEnvironment env)
    {
        var authority = configuration["Auth:Keycloak:Authority"]
                        ?? "http://localhost:5080/auth/realms/DevRealmDrivers";

        return builder.AddJwtBearer(AuthConstants.Schemes.Keycloak, options =>
        {
            options.Authority = authority;
            options.RequireHttpsMetadata = !env.IsDevelopment();
            options.TokenValidationParameters.ValidateAudience = false;
        });
    }

    public static void AddKeycloakSwaggerDefinition(this SwaggerGenOptions config, IConfiguration configuration)
    {
        var authority = (configuration["Auth:Keycloak:Authority"]
                         ?? "http://localhost:5080/auth/realms/DevRealmDrivers").TrimEnd('/');

        config.AddSecurityDefinition(AuthConstants.SwaggerSecurityIds.KeycloakOAuth2, new OpenApiSecurityScheme
        {
            Type = SecuritySchemeType.OAuth2,
            Description = "【03.3 Keycloak IAM】 Пряма автентифікація через Keycloak Realm DevRealmDrivers (alice/bob).",
            Flows = new OpenApiOAuthFlows
            {
                AuthorizationCode = new OpenApiOAuthFlow
                {
                    AuthorizationUrl = new Uri($"{authority}/protocol/openid-connect/auth"),
                    TokenUrl = new Uri($"{authority}/protocol/openid-connect/token"),
                    Scopes = new Dictionary<string, string>
                    {
                        { "openid", "OpenID Connect" },
                        { "profile", "Профіль користувача" },
                        { "drivers_management", "Доступ до DriversManagement" },
                        { "cargo_ordering", "Доступ до CargoOrderingManager" },
                        { "blog_management", "Доступ до DriversBlogManagement" }
                    }
                }
            }
        });
    }
}
