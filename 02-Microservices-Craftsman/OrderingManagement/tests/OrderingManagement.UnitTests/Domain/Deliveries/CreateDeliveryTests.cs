namespace OrderingManagement.UnitTests.Domain.Deliveries;

using OrderingManagement.SharedTestHelpers.Fakes.Delivery;
using OrderingManagement.Domain.Deliveries;
using OrderingManagement.Domain.Deliveries.DomainEvents;
using Bogus;
using FluentAssertions.Extensions;
using ValidationException = OrderingManagement.Exceptions.ValidationException;

public class CreateDeliveryTests
{
    private readonly Faker _faker;

    public CreateDeliveryTests()
    {
        _faker = new Faker();
    }
    
    [Fact]
    public void can_create_valid_delivery()
    {
        // Arrange
        var deliveryToCreate = new FakeDeliveryForCreation().Generate();
        
        // Act
        var delivery = Delivery.Create(deliveryToCreate);

        // Assert
        delivery.DeliveryDate.Should().Be(deliveryToCreate.DeliveryDate);
        delivery.PickupAddress.Should().Be(deliveryToCreate.PickupAddress);
        delivery.DestinationAddress.Should().Be(deliveryToCreate.DestinationAddress);
        delivery.PackageDetails.Should().Be(deliveryToCreate.PackageDetails);
        delivery.DeliveryStatus.Should().Be(deliveryToCreate.DeliveryStatus);
    }

    [Fact]
    public void queue_domain_event_on_create()
    {
        // Arrange
        var deliveryToCreate = new FakeDeliveryForCreation().Generate();
        
        // Act
        var delivery = Delivery.Create(deliveryToCreate);

        // Assert
        delivery.DomainEvents.Count.Should().Be(1);
        delivery.DomainEvents.FirstOrDefault().Should().BeOfType(typeof(DeliveryCreated));
    }
}