// ====================================================================================
// 🚀 ТОЧКА ВХОДУ PULUMI IaC (МОДУЛЬ 03.3 — KEYCLOAK ENTERPRISE IAM)
// ------------------------------------------------------------------------------------
// Цей консольний проєкт НЕ є веб-сервісом — його запускає CLI-утиліта `pulumi`
// (командами `pulumi preview` або `pulumi up`).
//
// Як це працює:
//   1. `pulumi` компілює цей проєкт і запускає `Main()`.
//   2. `Deployment.RunAsync<RealmBuild>()` створює екземпляр стеку `RealmBuild`
//      (див. RealmBuild.cs), збирає опис бажаного стану Keycloak (Realm, Scopes,
//      Clients, Mappers, Users) і порівнює його з поточним станом у Keycloak
//      через Admin REST API (адреса та креды беруться з Pulumi.dev.yaml).
//   3. Pulumi застосовує лише різницю (diff): створює відсутні або оновлює змінені
//      ресурси без перезапуску контейнера Keycloak.
// ====================================================================================

namespace KeycloakPulumiAuth;

using System.Threading.Tasks;
using Pulumi;

internal static class Program
{
    private static Task<int> Main() => Deployment.RunAsync<RealmBuild>();
}
