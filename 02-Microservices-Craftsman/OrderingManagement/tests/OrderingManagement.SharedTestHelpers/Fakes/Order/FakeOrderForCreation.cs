namespace OrderingManagement.SharedTestHelpers.Fakes.Order;

using AutoBogus;
using OrderingManagement.Domain.Orders;
using OrderingManagement.Domain.Orders.Models;

public sealed class FakeOrderForCreation : AutoFaker<OrderForCreation>
{
    public FakeOrderForCreation()
    {
    }
}