namespace OrderingManagement.SharedTestHelpers.Fakes.Order;

using AutoBogus;
using OrderingManagement.Domain.Orders;
using OrderingManagement.Domain.Orders.Dtos;

public sealed class FakeOrderForCreationDto : AutoFaker<OrderForCreationDto>
{
    public FakeOrderForCreationDto()
    {
    }
}