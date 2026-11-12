namespace OrderingManagement.Domain.Orders.Features;

using OrderingManagement.Domain.Orders.Dtos;
using OrderingManagement.Domain.Orders.Services;
using OrderingManagement.Exceptions;
using OrderingManagement.Resources;
using OrderingManagement.Domain;
using HeimGuard;
using Mappings;
using Microsoft.EntityFrameworkCore;
using MediatR;
using QueryKit;
using QueryKit.Configuration;

public static class GetOrderList
{
    public sealed record Query(OrderParametersDto QueryParameters) : IRequest<PagedList<OrderDto>>;

    public sealed class Handler : IRequestHandler<Query, PagedList<OrderDto>>
    {
        private readonly IOrderRepository _orderRepository;
        private readonly IHeimGuardClient _heimGuard;

        public Handler(IOrderRepository orderRepository, IHeimGuardClient heimGuard)
        {
            _orderRepository = orderRepository;
            _heimGuard = heimGuard;
        }

        public async Task<PagedList<OrderDto>> Handle(Query request, CancellationToken cancellationToken)
        {
            // [AUTH-DISABLED] await _heimGuard.MustHavePermission<ForbiddenAccessException>(Permissions.CanReadOrders);

            var collection = _orderRepository.Query().AsNoTracking();

            var queryKitConfig = new CustomQueryKitConfiguration();
            var queryKitData = new QueryKitData()
            {
                Filters = request.QueryParameters.Filters,
                SortOrder = request.QueryParameters.SortOrder,
                Configuration = queryKitConfig
            };
            var appliedCollection = collection.ApplyQueryKit(queryKitData);
            var dtoCollection = appliedCollection.ToOrderDtoQueryable();

            return await PagedList<OrderDto>.CreateAsync(dtoCollection,
                request.QueryParameters.PageNumber,
                request.QueryParameters.PageSize,
                cancellationToken);
        }
    }
}