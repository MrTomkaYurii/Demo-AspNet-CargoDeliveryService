namespace KeycloakPulumiAuth;

using System.Threading.Tasks;
using Pulumi;

// Точка входу Pulumi IaC (Модуль 03.3): розгортає декларативний стек RealmBuild у Keycloak
internal static class Program
{
    private static Task<int> Main() => Deployment.RunAsync<RealmBuild>();
}