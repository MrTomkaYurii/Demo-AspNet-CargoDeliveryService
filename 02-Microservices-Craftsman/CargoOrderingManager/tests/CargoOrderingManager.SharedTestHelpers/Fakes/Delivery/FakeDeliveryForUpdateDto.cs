namespace CargoOrderingManager.SharedTestHelpers.Fakes.Delivery;

using AutoBogus;
using CargoOrderingManager.Domain.Deliveries;
using CargoOrderingManager.Domain.Deliveries.Dtos;

public sealed class FakeDeliveryForUpdateDto : AutoFaker<DeliveryForUpdateDto>
{
    public FakeDeliveryForUpdateDto()
    {
    }
}