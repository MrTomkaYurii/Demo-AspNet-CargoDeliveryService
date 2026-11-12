namespace OrderingManagement.SharedTestHelpers.Fakes.Order;

using AutoBogus;
using OrderingManagement.Domain.Orders;
using OrderingManagement.Domain.Orders.Models;

public sealed class FakeOrderForUpdate : AutoFaker<OrderForUpdate>
{
    public FakeOrderForUpdate()
    {
    }
}