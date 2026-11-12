namespace BlogManagement.Domain.RolePermissions.Features;

using BlogManagement.Domain.RolePermissions.Services;
using BlogManagement.Domain.RolePermissions;
using BlogManagement.Domain.RolePermissions.Dtos;
using BlogManagement.Domain.RolePermissions.Models;
using BlogManagement.Services;
using BlogManagement.Exceptions;
using BlogManagement.Domain;
using HeimGuard;
using Mappings;
using MediatR;

public static class AddRolePermission
{
    public sealed record Command(RolePermissionForCreationDto RolePermissionToAdd) : IRequest<RolePermissionDto>;

    public sealed class Handler : IRequestHandler<Command, RolePermissionDto>
    {
        private readonly IRolePermissionRepository _rolePermissionRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IHeimGuardClient _heimGuard;

        public Handler(IRolePermissionRepository rolePermissionRepository, IUnitOfWork unitOfWork, IHeimGuardClient heimGuard)
        {
            _rolePermissionRepository = rolePermissionRepository;
            _unitOfWork = unitOfWork;
            _heimGuard = heimGuard;
        }

        public async Task<RolePermissionDto> Handle(Command request, CancellationToken cancellationToken)
        {
            // [AUTH-DISABLED] await _heimGuard.MustHavePermission<ForbiddenAccessException>(Permissions.CanAddRolePermissions);

            var rolePermissionToAdd = request.RolePermissionToAdd.ToRolePermissionForCreation();
            var rolePermission = RolePermission.Create(rolePermissionToAdd);

            await _rolePermissionRepository.Add(rolePermission, cancellationToken);
            await _unitOfWork.CommitChanges(cancellationToken);

            return rolePermission.ToRolePermissionDto();
        }
    }
}