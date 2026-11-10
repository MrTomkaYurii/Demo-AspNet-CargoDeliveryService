namespace CargoOrderingManager.SharedTestHelpers.Fakes.RolePermission;

using AutoBogus;
using CargoOrderingManager.Domain;
using CargoOrderingManager.Domain.RolePermissions.Dtos;
using CargoOrderingManager.Domain.Roles;
using CargoOrderingManager.Domain.RolePermissions.Models;

public sealed class FakeRolePermissionForUpdate : AutoFaker<RolePermissionForUpdate>
{
    public FakeRolePermissionForUpdate()
    {
        RuleFor(rp => rp.Permission, f => f.PickRandom(Permissions.List()));
        RuleFor(rp => rp.Role, f => f.PickRandom(Role.ListNames()));
    }
}