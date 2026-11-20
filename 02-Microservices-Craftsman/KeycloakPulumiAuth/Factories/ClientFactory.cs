// ====================================================================================
// 🏭 ФАБРИКА OIDC-КЛІЄНТІВ KEYCLOAK (Authorization Code + PKCE та Client Credentials)
// ------------------------------------------------------------------------------------
// Інкапсулює типові налаштування OpenID Connect клієнтів, щоб у RealmBuild.cs не
// дублювати десятки однакових властивостей для кожного мікросервісу.
// ====================================================================================

namespace KeycloakPulumiAuth.Factories;

using Pulumi;
using Pulumi.Keycloak.OpenId;

public class ClientFactory
{
    /// <summary>
    /// Створює інтерактивного OIDC-клієнта для Authorization Code Flow + PKCE (S256):
    /// використовується для Swagger UI, Postman (Code Flow) та BFF-застосунків.
    /// </summary>
    /// <param name="realmId">ID реалму Keycloak (Output&lt;string&gt; від ресурсу Realm).</param>
    /// <param name="clientId">Унікальний ClientId, який передаватиме клієнт у /auth та /token.</param>
    /// <param name="clientSecret">Секрет клієнта (оскільки AccessType = CONFIDENTIAL).</param>
    /// <param name="clientName">Читабельна назва клієнта в адмін-панелі Keycloak.</param>
    /// <param name="baseUrl">Базова адреса застосунку (використовується для Redirect URI та CORS за замовчуванням).</param>
    /// <param name="redirectUris">Явний список дозволених redirect_uri (якщо null — підставляється {baseUrl}/*).</param>
    /// <param name="webOrigins">Явний список CORS origins (якщо null — підставляється baseUrl).</param>
    public static Client CreateCodeFlowClient(Output<string> realmId,
        string clientId,
        string clientSecret,
        string clientName,
        string baseUrl,
        InputList<string>? redirectUris = null,
        InputList<string>? webOrigins = null)
    {
        return new Client($"{clientName.ToLower()}-client", new ClientArgs()
        {
            RealmId = realmId,
            ClientId = clientId,
            Name = clientName,
            StandardFlowEnabled = true,       // Вмикає Authorization Code Flow (інтерактивний вхід через браузер)
            Enabled = true,
            AccessType = "CONFIDENTIAL",      // Клієнт із ClientSecret (для публічних SPA/Swagger без секрету використовують "PUBLIC")
            BaseUrl = baseUrl,
            AdminUrl = baseUrl,
            ValidRedirectUris = redirectUris ?? new InputList<string>()
            {
                new Uri(new Uri(baseUrl), "*").ToString()
            },
            WebOrigins = webOrigins ?? new InputList<string>()
            {
                baseUrl
            },
            PkceCodeChallengeMethod = "S256", // Обов'язковий захист PKCE (SHA-256) від перехоплення authorization_code
            ClientSecret = clientSecret,
            BackchannelLogoutSessionRequired = true,
            BackchannelLogoutUrl = baseUrl
        });
    }

    /// <summary>
    /// Створює M2M-клієнта (Machine-to-Machine) для Client Credentials Flow:
    /// автентифікація сервер-сервер за client_id + client_secret без участі користувача.
    /// </summary>
    public static Client CreateClientCredentialsFlowClient(Output<string> realmId,
        string clientId,
        string clientSecret,
        string clientName,
        string baseUrl)
    {
        return new Client($"{clientName.ToLower()}-client", new ClientArgs()
        {
            RealmId = realmId,
            ClientId = clientId,
            Name = clientName,
            StandardFlowEnabled = false,      // Інтерактивний вхід користувача вимкнено
            Enabled = true,
            ServiceAccountsEnabled = true,    // Вмикає Client Credentials Flow (grant_type=client_credentials)
            AccessType = "CONFIDENTIAL",      // M2M-клієнт завжди автентифікується секретом
            BaseUrl = baseUrl,
            AdminUrl = baseUrl,
            ClientSecret = clientSecret,
            BackchannelLogoutSessionRequired = true,
            BackchannelLogoutUrl = baseUrl
        });
    }
}
