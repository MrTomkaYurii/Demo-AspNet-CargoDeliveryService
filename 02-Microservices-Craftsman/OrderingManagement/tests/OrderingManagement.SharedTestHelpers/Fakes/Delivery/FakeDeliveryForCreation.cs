namespace OrderingManagement.SharedTestHelpers.Fakes.Delivery;

using AutoBogus;
using OrderingManagement.Domain.Deliveries;
using OrderingManagement.Domain.Deliveries.Models;

public sealed class FakeDeliveryForCreation : AutoFaker<DeliveryForCreation>
{
    public FakeDeliveryForCreation()
    {
    }
}