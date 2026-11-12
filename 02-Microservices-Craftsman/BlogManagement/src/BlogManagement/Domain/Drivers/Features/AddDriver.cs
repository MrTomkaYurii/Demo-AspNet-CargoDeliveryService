namespace BlogManagement.Domain.Drivers.Features;

using BlogManagement.Domain.Drivers.Services;
using BlogManagement.Domain.Drivers;
using BlogManagement.Domain.Drivers.Dtos;
using BlogManagement.Domain.Drivers.Models;
using BlogManagement.Services;
using BlogManagement.Exceptions;
using BlogManagement.Domain;
using HeimGuard;
using Mappings;
using MediatR;

public static class AddDriver
{
    public sealed record Command(DriverForCreationDto DriverToAdd) : IRequest<DriverDto>;

    public sealed class Handler : IRequestHandler<Command, DriverDto>
    {
        private readonly IDriverRepository _driverRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IHeimGuardClient _heimGuard;

        public Handler(IDriverRepository driverRepository, IUnitOfWork unitOfWork, IHeimGuardClient heimGuard)
        {
            _driverRepository = driverRepository;
            _unitOfWork = unitOfWork;
            _heimGuard = heimGuard;
        }

        public async Task<DriverDto> Handle(Command request, CancellationToken cancellationToken)
        {
            // [AUTH-DISABLED] await _heimGuard.MustHavePermission<ForbiddenAccessException>(Permissions.CanAddDriver);

            var driverToAdd = request.DriverToAdd.ToDriverForCreation();
            var driver = Driver.Create(driverToAdd);

            await _driverRepository.Add(driver, cancellationToken);
            await _unitOfWork.CommitChanges(cancellationToken);

            return driver.ToDriverDto();
        }
    }
}