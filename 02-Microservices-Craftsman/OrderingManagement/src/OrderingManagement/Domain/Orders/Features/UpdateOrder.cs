namespace OrderingManagement.Domain.Orders.Features;

using OrderingManagement.Domain.Orders;
using OrderingManagement.Domain.Orders.Dtos;
using OrderingManagement.Domain.Orders.Services;
using OrderingManagement.Services;
using OrderingManagement.Domain.Orders.Models;
using OrderingManagement.Exceptions;
using OrderingManagement.Domain;
using HeimGuard;
using Mappings;
using MediatR;

public static class UpdateOrder
{
    public sealed record Command(Guid OrderId, OrderForUpdateDto UpdatedOrderData) : IRequest;

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
            // [AUTH-DISABLED] await _heimGuard.MustHavePermission<ForbiddenAccessException>(Permissions.CanUpdateOrder);

            var orderToUpdate = await _orderRepository.GetById(request.OrderId, cancellationToken: cancellationToken);
            var orderToAdd = request.UpdatedOrderData.ToOrderForUpdate();
            orderToUpdate.Update(orderToAdd);

            _orderRepository.Update(orderToUpdate);
            await _unitOfWork.CommitChanges(cancellationToken);
        }
    }
}