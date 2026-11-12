namespace OrderingManagement.SharedTestHelpers.Fakes.Delivery;

using AutoBogus;
using OrderingManagement.Domain.Deliveries;
using OrderingManagement.Domain.Deliveries.Models;

public sealed class FakeDeliveryForUpdate : AutoFaker<DeliveryForUpdate>
{
    public FakeDeliveryForUpdate()
    {
    }
}