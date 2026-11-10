namespace CargoOrderingManager.Domain.Orders.Features;

using CargoOrderingManager.Domain.Orders.Services;
using CargoOrderingManager.Services;
using CargoOrderingManager.Exceptions;
using CargoOrderingManager.Domain;
using HeimGuard;
using MediatR;

public static class DeleteOrder
{
    public sealed record Command(Guid OrderId) : IRequest;

    public sealed class Handler : IRequestHandler<Command>
    {
        private readonly IOrderRepository _orderRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IHeimGuardClient _heimGuard;

        public Handler(IOrderRepository orderRepository, IUnitOfWork unitOfWork, IHeimGuardClient heimGuard)
        {
            _orderRepository = orderRepository;
            _unitOfWork = unitOfWork;
            _heimGuard = heimGuard;
        }

        public async Task Handle(Command request, CancellationToken cancellationToken)
        {
            // [AUTH-DISABLED] await _heimGuard.MustHavePermission<ForbiddenAccessException>(Permissions.CanDeleteOrder);

            var recordToDelete = await _orderRepository.GetById(request.OrderId, cancellationToken: cancellationToken);
            _orderRepository.Remove(recordToDelete);
            await _unitOfWork.CommitChanges(cancellationToken);
        }
    }
}