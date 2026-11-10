namespace CargoOrderingManager.SharedTestHelpers.Fakes.Order;

using AutoBogus;
using CargoOrderingManager.Domain.Orders;
using CargoOrderingManager.Domain.Orders.Dtos;

public sealed class FakeOrderForUpdateDto : AutoFaker<OrderForUpdateDto>
{
    public FakeOrderForUpdateDto()
    {
    }
}