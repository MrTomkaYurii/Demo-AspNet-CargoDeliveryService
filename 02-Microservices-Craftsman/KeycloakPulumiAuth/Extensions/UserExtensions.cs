// ====================================================================================
// 👤 МЕТОДИ-РОЗШИРЕННЯ ДЛЯ ПРИЗНАЧЕННЯ РОЛЕЙ КОРИСТУВАЧАМ KEYCLOAK
// ------------------------------------------------------------------------------------
// Прив'язує список Realm/Client ролей (за їх RoleIds) до конкретного користувача
// у реалмі Keycloak через ресурс Pulumi.Keycloak.UserRoles.
// ====================================================================================

namespace KeycloakPulumiAuth.Extensions;

using Pulumi;
using Pulumi.Keycloak;

public static class UserExtensions
{
    /// <summary>
    /// Призначає користувачу вказаний набір ролей Keycloak (за їхніми ID).
    /// </summary>
    public static UserRoles SetRoles(this User user, params Input<string>[] userRoles)
    {
        return new UserRoles($"user-roles-{user.Id}", new UserRolesArgs()
        {
            UserId = user.Id,
            RealmId = user.RealmId,
            RoleIds = userRoles
        });
    }
}
