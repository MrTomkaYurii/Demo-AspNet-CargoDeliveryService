namespace OrderingManagement.SharedTestHelpers.Fakes.Delivery;

using AutoBogus;
using OrderingManagement.Domain.Deliveries;
using OrderingManagement.Domain.Deliveries.Dtos;

public sealed class FakeDeliveryForUpdateDto : AutoFaker<DeliveryForUpdateDto>
{
    public FakeDeliveryForUpdateDto()
    {
    }
}