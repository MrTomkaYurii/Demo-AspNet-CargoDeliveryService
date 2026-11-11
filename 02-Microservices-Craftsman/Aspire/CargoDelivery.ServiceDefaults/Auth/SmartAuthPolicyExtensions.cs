// ====================================================================================
// 🧠 РОЗУМНИЙ МАРШРУТИЗАТОР СХЕМ (PolicyScheme — Одночасна робота 03.1 / 03.2 / 03.3)
// ------------------------------------------------------------------------------------
// Замість перезапуску мікросервісу при переході від одного провайдера до іншого,
// SmartAuthPolicy читає заголовок Authorization: Bearer <jwt>, декодує claim `iss`
// (Issuer) без перевірки підпису та спрямовує валідацію у відповідну схему:
//   1. iss == "AuthorizationModuleAPI"  → SimpleJwtScheme (03.1)
//   2. iss містить "/realms/"           → KeycloakScheme  (03.3)
//   3. Будь-який інший OIDC Issuer      → DuendeScheme    (03.2 або 03.3-Federation)
// ====================================================================================

using System.IdentityModel.Tokens.Jwt;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace CargoDelivery.ServiceDefaults.Auth;

public static class SmartAuthPolicyExtensions
{
    public static AuthenticationBuilder AddSmartPolicySelector(
        this AuthenticationBuilder builder,
        IConfiguration configuration)
    {
        var simpleIssuer = configuration["Auth:SimpleJwt:Issuer"] ?? SimpleJwtAuthExtensions.DefaultIssuer;

        return builder.AddPolicyScheme(
            AuthConstants.Schemes.SmartSelector,
            "Smart Multi-Provider JWT Selector (03.1 / 03.2 / 03.3)",
            options =>
            {
                options.ForwardDefaultSelector = context =>
                {
                    var authHeader = context.Request.Headers.Authorization.FirstOrDefault();
                    return SelectSchemeFromHeader(authHeader, simpleIssuer);
                };
            });
    }

    /// <summary>
    /// Визначає цільову схему автентифікації за полем `iss` (Issuer) всередині JWT.
    /// Винесено в окремий чистий метод для зручного модульного тестування.
    /// </summary>
    public static string SelectSchemeFromHeader(string? authorizationHeader, string simpleJwtIssuer = SimpleJwtAuthExtensions.DefaultIssuer)
    {
        if (string.IsNullOrWhiteSpace(authorizationHeader) ||
            !authorizationHeader.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
        {
            return AuthConstants.Schemes.Duende;
        }

        var token = authorizationHeader["Bearer ".Length..].Trim();
        var handler = new JwtSecurityTokenHandler();

        if (!handler.CanReadToken(token))
            return AuthConstants.Schemes.Duende;

        var jwt = handler.ReadJwtToken(token);
        var issuer = jwt.Issuer ?? string.Empty;

        // 03.1: Токен виписаний самописним AuthorizationModuleAPI
        if (string.Equals(issuer, simpleJwtIssuer, StringComparison.OrdinalIgnoreCase))
            return AuthConstants.Schemes.SimpleJwt;

        // 03.3: Токен виписаний напряму Keycloak
        if (issuer.Contains("/realms/", StringComparison.OrdinalIgnoreCase))
            return AuthConstants.Schemes.Keycloak;

        // 03.2 (+ Федерація 03.3 через Duende): Токен виписаний Duende IdentityServer
        return AuthConstants.Schemes.Duende;
    }
}
