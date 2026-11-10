namespace CargoOrderingManager.SharedTestHelpers.Fakes.Order;

using AutoBogus;
using CargoOrderingManager.Domain.Orders;
using CargoOrderingManager.Domain.Orders.Models;

public sealed class FakeOrderForUpdate : AutoFaker<OrderForUpdate>
{
    public FakeOrderForUpdate()
    {
    }
}