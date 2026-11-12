namespace OrderingManagement.Domain.Deliveries.Features;

using OrderingManagement.Domain.Deliveries.Dtos;
using OrderingManagement.Domain.Deliveries.Services;
using OrderingManagement.Exceptions;
using OrderingManagement.Resources;
using OrderingManagement.Domain;
using HeimGuard;
using Mappings;
using Microsoft.EntityFrameworkCore;
using MediatR;
using QueryKit;
using QueryKit.Configuration;

public static class GetDeliveryList
{
    public sealed record Query(DeliveryParametersDto QueryParameters) : IRequest<PagedList<DeliveryDto>>;

    public sealed class Handler : IRequestHandler<Query, PagedList<DeliveryDto>>
    {
        private readonly IDeliveryRepository _deliveryRepository;
        private readonly IHeimGuardClient _heimGuard;

        public Handler(IDeliveryRepository deliveryRepository, IHeimGuardClient heimGuard)
        {
            _deliveryRepository = deliveryRepository;
            _heimGuard = heimGuard;
        }

        public async Task<PagedList<DeliveryDto>> Handle(Query request, CancellationToken cancellationToken)
        {
            // [AUTH-DISABLED] await _heimGuard.MustHavePermission<ForbiddenAccessException>(Permissions.CanReadDeliveries);

            var collection = _deliveryRepository.Query().AsNoTracking();

            var queryKitConfig = new CustomQueryKitConfiguration();
            var queryKitData = new QueryKitData()
            {
                Filters = request.QueryParameters.Filters,
                SortOrder = request.QueryParameters.SortOrder,
                Configuration = queryKitConfig
            };
            var appliedCollection = collection.ApplyQueryKit(queryKitData);
            var dtoCollection = appliedCollection.ToDeliveryDtoQueryable();

            return await PagedList<DeliveryDto>.CreateAsync(dtoCollection,
                request.QueryParameters.PageNumber,
                request.QueryParameters.PageSize,
                cancellationToken);
        }
    }
}