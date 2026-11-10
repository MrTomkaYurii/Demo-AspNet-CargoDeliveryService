namespace CargoOrderingManager.Domain.Deliveries.Features;

using CargoOrderingManager.Domain.Deliveries.Dtos;
using CargoOrderingManager.Domain.Deliveries.Services;
using CargoOrderingManager.Exceptions;
using CargoOrderingManager.Domain;
using HeimGuard;
using Mappings;
using MediatR;

public static class GetDelivery
{
    public sealed record Query(Guid DeliveryId) : IRequest<DeliveryDto>;

    public sealed class Handler : IRequestHandler<Query, DeliveryDto>
    {
        private readonly IDeliveryRepository _deliveryRepository;
        private readonly IHeimGuardClient _heimGuard;

        public Handler(IDeliveryRepository deliveryRepository, IHeimGuardClient heimGuard)
        {
            _deliveryRepository = deliveryRepository;
            _heimGuard = heimGuard;
        }

        public async Task<DeliveryDto> Handle(Query request, CancellationToken cancellationToken)
        {
            // [AUTH-DISABLED] await _heimGuard.MustHavePermission<ForbiddenAccessException>(Permissions.CanReadDelivery);

            var result = await _deliveryRepository.GetById(request.DeliveryId, cancellationToken: cancellationToken);
            return result.ToDeliveryDto();
        }
    }
}