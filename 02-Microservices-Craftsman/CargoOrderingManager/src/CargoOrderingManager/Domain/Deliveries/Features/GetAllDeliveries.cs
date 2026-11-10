namespace CargoOrderingManager.Domain.Deliveries.Features;

using CargoOrderingManager.Domain.Deliveries.Dtos;
using CargoOrderingManager.Domain.Deliveries.Services;
using CargoOrderingManager.Exceptions;
using CargoOrderingManager.Resources;
using CargoOrderingManager.Domain;
using HeimGuard;
using Mappings;
using Microsoft.EntityFrameworkCore;
using MediatR;
using QueryKit;
using QueryKit.Configuration;

public static class GetAllDeliveries
{
    public sealed record Query() : IRequest<List<DeliveryDto>>;

    public sealed class Handler : IRequestHandler<Query, List<DeliveryDto>>
    {
        private readonly IDeliveryRepository _deliveryRepository;
        private readonly IHeimGuardClient _heimGuard;

        public Handler(IDeliveryRepository deliveryRepository, IHeimGuardClient heimGuard)
        {
            _deliveryRepository = deliveryRepository;
            _heimGuard = heimGuard;
        }

        public async Task<List<DeliveryDto>> Handle(Query request, CancellationToken cancellationToken)
        {
            // [AUTH-DISABLED] await _heimGuard.MustHavePermission<ForbiddenAccessException>(Permissions.CanReadDeliveries);

            return _deliveryRepository.Query()
                .AsNoTracking()
                .ToDeliveryDtoQueryable()
                .ToList();
        }
    }
}