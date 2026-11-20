// ====================================================================================
// 🧩 МЕТОДИ-РОЗШИРЕННЯ ДЛЯ НАЛАШТУВАННЯ OIDC-КЛІЄНТІВ KEYCLOAK
// ------------------------------------------------------------------------------------
// Дозволяють декларативно додавати клієнту:
//   1. Default Scopes — скоупи, які Keycloak автоматично включає в кожен виданий токен.
//   2. Audience Protocol Mapper — додає цільовий мікросервіс у клейм "aud" Access Token,
//      щоб API міг перевіряти, чи призначений цей токен саме для нього.
// ====================================================================================

namespace KeycloakPulumiAuth.Extensions;

using Pulumi;
using Pulumi.Keycloak.OpenId;

public static class ClientExtensions
{
    /// <summary>
    /// Призначає клієнту базові OIDC-скоупи (openid, profile, email, roles, web-origins)
    /// разом із кастомними скоупами мікросервісів (drivers_management, cargo_ordering тощо).
    /// </summary>
    public static void ExtendDefaultScopes(this Client client, params Output<string>[] scopeNames)
    {
        // .Apply(...) очікує резолву асинхронного Output<string> від Pulumi перед створенням залежного ресурсу
        var defaultScopes = client.Name.Apply(clientName =>
            new ClientDefaultScopes($"default-scopes-for-{clientName}", new ClientDefaultScopesArgs()
            {
                RealmId = client.RealmId,
                ClientId = client.Id,
                DefaultScopes =
                {
                    "openid",
                    "profile",
                    "email",
                    "roles",
                    "web-origins",
                    scopeNames,
                },
            })
        );
    }

    /// <summary>
    /// Додає Audience Protocol Mapper до клієнта: Keycloak автоматично запише значення
    /// <paramref name="audience"/> у поле "aud" (Audience) виданого JWT Access Token.
    /// </summary>
    public static void AddAudienceMapper(this Client client, string audience)
    {
        var audienceMapper = client.Name.Apply(clientName =>
            new AudienceProtocolMapper($"audienceMapper-{clientName}-{audience}", new AudienceProtocolMapperArgs
            {
                RealmId = client.RealmId,
                ClientId = client.Id,
                IncludedCustomAudience = audience,
                Name = $"{audience}-Mapping"
            })
        );
    }
}
