namespace OrderingManagement.Domain.Deliveries.Mappings;

using OrderingManagement.Domain.Deliveries.Dtos;
using OrderingManagement.Domain.Deliveries.Models;
using Riok.Mapperly.Abstractions;

[Mapper]
public static partial class DeliveryMapper
{
    public static partial DeliveryForCreation ToDeliveryForCreation(this DeliveryForCreationDto deliveryForCreationDto);
    public static partial DeliveryForUpdate ToDeliveryForUpdate(this DeliveryForUpdateDto deliveryForUpdateDto);
    public static partial DeliveryDto ToDeliveryDto(this Delivery delivery);
    public static partial IQueryable<DeliveryDto> ToDeliveryDtoQueryable(this IQueryable<Delivery> delivery);
}