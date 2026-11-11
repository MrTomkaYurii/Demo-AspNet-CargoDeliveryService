// ====================================================================================
// 🛡️ ЄДИНИЙ ФАСАД ПІДКЛЮЧЕННЯ БЕЗПЕКИ ДЛЯ МІКРОСЕРВІСІВ CRAFTSMAN
// ------------------------------------------------------------------------------------
// Інкапсулює налаштування автентифікації, Swagger UI (3 провайдери одночасно)
// та прокидання Bearer-токена між сервісами (AuthorizationForwardingHandler).
//
// Використання в будь-якому мікросервісі:
//   builder.Services.AddCargoMultiAuth(builder.Configuration, builder.Environment);
//   config.AddCargoSwaggerMultiAuth(configuration);
//   uiConfig.UseCargoSwaggerMultiAuth(configuration);
// ====================================================================================

using System.Net.Http.Headers;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;
using Swashbuckle.AspNetCore.SwaggerUI;

namespace CargoDelivery.ServiceDefaults.Auth;

public static class CargoAuthServiceExtensions
{
    /// <summary>
    /// Реєструє автентифікацію відповідно до Auth:Mode (за замовчуванням "Multi" — усі 3 провайдери одночасно).
    /// </summary>
    public static IServiceCollection AddCargoMultiAuth(
        this IServiceCollection services,
        IConfiguration configuration,
        IHostEnvironment env)
    {
        var mode = configuration["Auth:Mode"] ?? AuthConstants.Modes.Multi;

        if (string.Equals(mode, AuthConstants.Modes.None, StringComparison.OrdinalIgnoreCase))
            return services;

        var defaultScheme = mode switch
        {
            AuthConstants.Modes.SimpleJwt => AuthConstants.Schemes.SimpleJwt,
            AuthConstants.Modes.Duende => AuthConstants.Schemes.Duende,
            AuthConstants.Modes.Keycloak => AuthConstants.Schemes.Keycloak,
            _ => AuthConstants.Schemes.SmartSelector
        };

        var authBuilder = services.AddAuthentication(options =>
        {
            options.DefaultAuthenticateScheme = defaultScheme;
            options.DefaultChallengeScheme = defaultScheme;
            options.DefaultScheme = defaultScheme;
        });

        // Реєструємо всі три схеми незалежно одна від одної
        authBuilder.AddSimpleJwtScheme(configuration);
        authBuilder.AddDuendeScheme(configuration, env);
        authBuilder.AddKeycloakScheme(configuration, env);

        if (defaultScheme == AuthConstants.Schemes.SmartSelector)
        {
            authBuilder.AddSmartPolicySelector(configuration);
        }

        services.AddHttpContextAccessor();
        services.AddTransient<AuthorizationForwardingHandler>();

        return services;
    }

    /// <summary>
    /// Додає до Swagger UI усі 3 способи авторизації (03.1 Simple Bearer, 03.2 Duende PKCE, 03.3 Keycloak PKCE).
    /// </summary>
    public static void AddCargoSwaggerMultiAuth(this SwaggerGenOptions config, IConfiguration configuration)
    {
        config.AddSimpleJwtSwaggerDefinition();
        config.AddDuendeSwaggerDefinition(configuration);
        config.AddKeycloakSwaggerDefinition(configuration);

        config.AddSecurityRequirement(new OpenApiSecurityRequirement
        {
            {
                new OpenApiSecurityScheme
                {
                    Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = AuthConstants.SwaggerSecurityIds.SimpleBearer }
                },
                Array.Empty<string>()
            },
            {
                new OpenApiSecurityScheme
                {
                    Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = AuthConstants.SwaggerSecurityIds.DuendeOAuth2 }
                },
                new[] { "openid", "profile", "email", "drivers_management", "cargo_ordering", "blog_management" }
            },
            {
                new OpenApiSecurityScheme
                {
                    Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = AuthConstants.SwaggerSecurityIds.KeycloakOAuth2 }
                },
                new[] { "openid", "profile", "email", "drivers_management", "cargo_ordering", "blog_management" }
            }
        });
    }

    /// <summary>
    /// Налаштовує OAuth2 PKCE клієнтські параметри в Swagger UI для Duende та Keycloak.
    /// </summary>
    public static void UseCargoSwaggerMultiAuth(this SwaggerUIOptions config, IConfiguration configuration)
    {
        config.OAuthClientId("swagger");
        config.OAuthAppName("CargoDelivery Platform — Multi-Auth Demo (03.1 / 03.2 / 03.3)");
        config.OAuthUsePkce();
    }
}

/// <summary>
/// DelegatingHandler для наскрізного прокидання заголовка Authorization: Bearer ...
/// від вхідного HTTP-запиту (наприклад, у HttpAggregator) до внутрішніх мікросервісів.
/// </summary>
public sealed class AuthorizationForwardingHandler : DelegatingHandler
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public AuthorizationForwardingHandler(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        var authHeader = _httpContextAccessor.HttpContext?.Request.Headers.Authorization.FirstOrDefault();
        if (!string.IsNullOrWhiteSpace(authHeader) &&
            AuthenticationHeaderValue.TryParse(authHeader, out var parsedHeader))
        {
            request.Headers.Authorization = parsedHeader;
        }

        return base.SendAsync(request, cancellationToken);
    }
}
