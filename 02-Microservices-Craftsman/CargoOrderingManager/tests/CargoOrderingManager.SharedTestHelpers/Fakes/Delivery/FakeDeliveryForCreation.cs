namespace CargoOrderingManager.SharedTestHelpers.Fakes.Delivery;

using AutoBogus;
using CargoOrderingManager.Domain.Deliveries;
using CargoOrderingManager.Domain.Deliveries.Models;

public sealed class FakeDeliveryForCreation : AutoFaker<DeliveryForCreation>
{
    public FakeDeliveryForCreation()
    {
    }
}