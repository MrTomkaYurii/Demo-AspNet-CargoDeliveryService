namespace OrderingManagement.Domain.Deliveries.Features;

using OrderingManagement.Domain.Deliveries.Services;
using OrderingManagement.Services;
using OrderingManagement.Exceptions;
using OrderingManagement.Domain;
using HeimGuard;
using MediatR;

public static class DeleteDelivery
{
    public sealed record Command(Guid DeliveryId) : IRequest;

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
            // [AUTH-DISABLED] await _heimGuard.MustHavePermission<ForbiddenAccessException>(Permissions.CanDeleteDelivery);

            var recordToDelete = await _deliveryRepository.GetById(request.DeliveryId, cancellationToken: cancellationToken);
            _deliveryRepository.Remove(recordToDelete);
            await _unitOfWork.CommitChanges(cancellationToken);
        }
    }
}