namespace BlogManagement.SharedTestHelpers.Fakes.RolePermission;

using AutoBogus;
using BlogManagement.Domain;
using BlogManagement.Domain.RolePermissions.Dtos;
using BlogManagement.Domain.Roles;
using BlogManagement.Domain.RolePermissions.Models;

public sealed class FakeRolePermissionForUpdateDto : AutoFaker<RolePermissionForUpdateDto>
{
    public FakeRolePermissionForUpdateDto()
    {
        RuleFor(rp => rp.Permission, f => f.PickRandom(Permissions.List()));
        RuleFor(rp => rp.Role, f => f.PickRandom(Role.ListNames()));
    }
}