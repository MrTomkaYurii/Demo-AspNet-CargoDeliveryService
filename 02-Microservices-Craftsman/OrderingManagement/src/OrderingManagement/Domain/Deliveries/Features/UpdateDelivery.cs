namespace OrderingManagement.Domain.Deliveries.Features;

using OrderingManagement.Domain.Deliveries;
using OrderingManagement.Domain.Deliveries.Dtos;
using OrderingManagement.Domain.Deliveries.Services;
using OrderingManagement.Services;
using OrderingManagement.Domain.Deliveries.Models;
using OrderingManagement.Exceptions;
using OrderingManagement.Domain;
using HeimGuard;
using Mappings;
using MediatR;

public static class UpdateDelivery
{
    public sealed record Command(Guid DeliveryId, DeliveryForUpdateDto UpdatedDeliveryData) : IRequest;

    public sealed class Handler : IRequestHandler<Command>
    {
        private readonly IDeliveryRepository _deliveryRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IHeimGuardClient _heimGuard;

        public Handler(IDeliveryRepository deliveryRepository, IUnitOfWork unitOfWork, IHeimGuardClient heimGuard)
        {
            _deliveryRepository = deliveryRepository;
            _unitOfWork = unitOfWork;
            _heimGuard = heimGuard;
        }

        public async Task Handle(Command request, CancellationToken cancellationToken)
        {
            // [AUTH-DISABLED] await _heimGuard.MustHavePermission<ForbiddenAccessException>(Permissions.CanUpdateDelivery);

            var deliveryToUpdate = await _deliveryRepository.GetById(request.DeliveryId, cancellationToken: cancellationToken);
            var deliveryToAdd = request.UpdatedDeliveryData.ToDeliveryForUpdate();
            deliveryToUpdate.Update(deliveryToAdd);

            _deliveryRepository.Update(deliveryToUpdate);
            await _unitOfWork.CommitChanges(cancellationToken);
        }
    }
}