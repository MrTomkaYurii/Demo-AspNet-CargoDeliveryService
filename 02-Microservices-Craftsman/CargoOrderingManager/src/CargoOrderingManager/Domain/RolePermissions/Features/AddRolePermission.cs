namespace CargoOrderingManager.Domain.RolePermissions.Features;

using CargoOrderingManager.Domain.RolePermissions.Services;
using CargoOrderingManager.Domain.RolePermissions;
using CargoOrderingManager.Domain.RolePermissions.Dtos;
using CargoOrderingManager.Domain.RolePermissions.Models;
using CargoOrderingManager.Services;
using CargoOrderingManager.Exceptions;
using CargoOrderingManager.Domain;
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