namespace CargoOrderingManager.UnitTests.Domain.Orders;

using CargoOrderingManager.SharedTestHelpers.Fakes.Order;
using CargoOrderingManager.Domain.Orders;
using CargoOrderingManager.Domain.Orders.DomainEvents;
using Bogus;
using FluentAssertions.Extensions;
using ValidationException = CargoOrderingManager.Exceptions.ValidationException;

public class CreateOrderTests
{
    private readonly Faker _faker;

    public CreateOrderTests()
    {
        _faker = new Faker();
    }
    
    [Fact]
    public void can_create_valid_order()
    {
        // Arrange
        var orderToCreate = new FakeOrderForCreation().Generate();
        
        // Act
        var order = Order.Create(orderToCreate);

        // Assert
        order.OrderNumber.Should().Be(orderToCreate.OrderNumber);
        order.CustomerName.Should().Be(orderToCreate.CustomerName);
        order.DeliveryDate.Should().Be(orderToCreate.DeliveryDate);
        order.TotalAmount.Should().Be(orderToCreate.TotalAmount);
        order.Status.Should().Be(orderToCreate.Status);
    }

    [Fact]
    public void queue_domain_event_on_create()
    {
        // Arrange
        var orderToCreate = new FakeOrderForCreation().Generate();
        
        // Act
        var order = Order.Create(orderToCreate);

        // Assert
        order.DomainEvents.Count.Should().Be(1);
        order.DomainEvents.FirstOrDefault().Should().BeOfType(typeof(OrderCreated));
    }
}