namespace CargoOrderingManager.Domain.RolePermissions.Features;

using CargoOrderingManager.Domain.RolePermissions;
using CargoOrderingManager.Domain.RolePermissions.Dtos;
using CargoOrderingManager.Domain.RolePermissions.Services;
using CargoOrderingManager.Services;
using CargoOrderingManager.Domain.RolePermissions.Models;
using CargoOrderingManager.Exceptions;
using CargoOrderingManager.Domain;
using HeimGuard;
using Mappings;
using MediatR;

public static class UpdateRolePermission
{
    public sealed record Command(Guid RolePermissionId, RolePermissionForUpdateDto UpdatedRolePermissionData) : IRequest;

    public sealed class Handler : IRequestHandler<Command>
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

        public async Task Handle(Command request, CancellationToken cancellationToken)
        {
            // [AUTH-DISABLED] await _heimGuard.MustHavePermission<ForbiddenAccessException>(Permissions.CanUpdateRolePermissions);

            var rolePermissionToUpdate = await _rolePermissionRepository.GetById(request.RolePermissionId, cancellationToken: cancellationToken);
            var rolePermissionToAdd = request.UpdatedRolePermissionData.ToRolePermissionForUpdate();
            rolePermissionToUpdate.Update(rolePermissionToAdd);

            _rolePermissionRepository.Update(rolePermissionToUpdate);
            await _unitOfWork.CommitChanges(cancellationToken);
        }
    }
}