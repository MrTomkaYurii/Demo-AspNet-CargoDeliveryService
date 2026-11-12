namespace OrderingManagement.SharedTestHelpers.Fakes.Order;

using AutoBogus;
using OrderingManagement.Domain.Orders;
using OrderingManagement.Domain.Orders.Dtos;

public sealed class FakeOrderForUpdateDto : AutoFaker<OrderForUpdateDto>
{
    public FakeOrderForUpdateDto()
    {
    }
}