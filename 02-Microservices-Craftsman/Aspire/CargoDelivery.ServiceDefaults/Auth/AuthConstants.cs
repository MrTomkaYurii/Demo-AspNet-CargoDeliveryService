// ====================================================================================
// 🔐 КОНСТАНТИ ТА РЕЖИМИ ПІДСИСТЕМИ АВТЕНТИФІКАЦІЇ (03.1 / 03.2 / 03.3)
// ------------------------------------------------------------------------------------
// Жорстке розмежування імен схем автентифікації та режимів роботи дозволяє
// незалежно демонструвати та тестувати кожен з трьох модулів безпеки:
//   • 03.1 SimpleJwt — самописний AuthorizationModuleAPI (HMAC-SHA256)
//   • 03.2 Duende    — Duende IdentityServer (OIDC / OAuth 2.0 + PKCE)
//   • 03.3 Keycloak  — Enterprise IAM Keycloak + Pulumi IaC + Федерація у Duende
// ====================================================================================

namespace CargoDelivery.ServiceDefaults.Auth;

public static class AuthConstants
{
    public static class Schemes
    {
        /// <summary>Розумний маршрутизатор (PolicyScheme), який визначає провайдера за claim `iss`.</summary>
        public const string SmartSelector = "SmartJwtSelector";

        /// <summary>03.1 — Схема для токенів від AuthorizationModuleAPI (симетричний ключ).</summary>
        public const string SimpleJwt = "SimpleJwtScheme";

        /// <summary>03.2 — Схема для токенів від Duende IdentityServer (RS256 / at+jwt).</summary>
        public const string Duende = "DuendeScheme";

        /// <summary>03.3 — Схема для токенів від Keycloak (Realm DevRealmDrivers).</summary>
        public const string Keycloak = "KeycloakScheme";
    }

    public static class Modes
    {
        /// <summary>Одночасна робота всіх трьох провайдерів (рекомендовано для живого демо).</summary>
        public const string Multi = "Multi";

        /// <summary>Ізольований режим 03.1: тільки AuthorizationModuleAPI.</summary>
        public const string SimpleJwt = "SimpleJwt";

        /// <summary>Ізольований режим 03.2: тільки Duende IdentityServer.</summary>
        public const string Duende = "Duende";

        /// <summary>Ізольований режим 03.3: тільки Keycloak.</summary>
        public const string Keycloak = "Keycloak";

        /// <summary>Вимкнено (для зворотної сумісності або спец-тестів).</summary>
        public const string None = "None";
    }

    public static class SwaggerSecurityIds
    {
        public const string SimpleBearer = "03.1-SimpleJWT-Bearer";
        public const string DuendeOAuth2 = "03.2-Duende-OAuth2";
        public const string KeycloakOAuth2 = "03.3-Keycloak-OAuth2";
    }
}
