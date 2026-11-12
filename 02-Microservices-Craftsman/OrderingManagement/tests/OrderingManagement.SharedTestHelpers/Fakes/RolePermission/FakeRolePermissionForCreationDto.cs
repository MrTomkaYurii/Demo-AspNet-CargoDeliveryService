namespace OrderingManagement.SharedTestHelpers.Fakes.RolePermission;

using AutoBogus;
using OrderingManagement.Domain;
using OrderingManagement.Domain.RolePermissions.Dtos;
using OrderingManagement.Domain.Roles;
using OrderingManagement.Domain.RolePermissions.Models;

public sealed class FakeRolePermissionForCreationDto : AutoFaker<RolePermissionForCreationDto>
{
    public FakeRolePermissionForCreationDto()
    {
        RuleFor(rp => rp.Permission, f => f.PickRandom(Permissions.List()));
        RuleFor(rp => rp.Role, f => f.PickRandom(Role.ListNames()));
    }
}