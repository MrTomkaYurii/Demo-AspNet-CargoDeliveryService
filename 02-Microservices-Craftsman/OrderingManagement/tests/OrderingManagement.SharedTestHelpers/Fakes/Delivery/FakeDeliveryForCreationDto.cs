namespace OrderingManagement.SharedTestHelpers.Fakes.Delivery;

using AutoBogus;
using OrderingManagement.Domain.Deliveries;
using OrderingManagement.Domain.Deliveries.Dtos;

public sealed class FakeDeliveryForCreationDto : AutoFaker<DeliveryForCreationDto>
{
    public FakeDeliveryForCreationDto()
    {
    }
}