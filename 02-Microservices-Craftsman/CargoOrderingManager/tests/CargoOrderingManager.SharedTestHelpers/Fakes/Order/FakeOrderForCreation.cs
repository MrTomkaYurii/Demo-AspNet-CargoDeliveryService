namespace CargoOrderingManager.SharedTestHelpers.Fakes.Order;

using AutoBogus;
using CargoOrderingManager.Domain.Orders;
using CargoOrderingManager.Domain.Orders.Models;

public sealed class FakeOrderForCreation : AutoFaker<OrderForCreation>
{
    public FakeOrderForCreation()
    {
    }
}