// ====================================================================================
// 🎯 ФАБРИКА OIDC CLIENT SCOPES У KEYCLOAK
// ------------------------------------------------------------------------------------
// Створює кастомні скоупи доступу до мікросервісів (наприклад, drivers_management,
// blog_management, cargo_ordering), які потім прив'язуються до клієнтів через
// ClientExtensions.ExtendDefaultScopes(...).
// ====================================================================================

namespace KeycloakPulumiAuth.Factories;

using Pulumi;
using Pulumi.Keycloak.OpenId;

public class ScopeFactory
{
    /// <summary>
    /// Реєструє новий OpenID Connect Client Scope всередині вказаного реалму Keycloak.
    /// </summary>
    public static ClientScope CreateScope(Output<string> realmId, string scopeName)
    {
        return new ClientScope($"{scopeName}-scope", new ClientScopeArgs()
        {
            Name = scopeName,
            RealmId = realmId,
        });
    }
}
