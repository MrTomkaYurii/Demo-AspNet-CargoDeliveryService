namespace BlogManagement.UnitTests.Domain.Drivers;

using BlogManagement.SharedTestHelpers.Fakes.Driver;
using BlogManagement.Domain.Drivers;
using BlogManagement.Domain.Drivers.DomainEvents;
using Bogus;
using FluentAssertions.Extensions;
using ValidationException = BlogManagement.Exceptions.ValidationException;

public class UpdateDriverTests
{
    private readonly Faker _faker;

    public UpdateDriverTests()
    {
        _faker = new Faker();
    }
    
    [Fact]
    public void can_update_driver()
    {
        // Arrange
        var driver = new FakeDriverBuilder().Build();
        var updatedDriver = new FakeDriverForUpdate().Generate();
        
        // Act
        driver.Update(updatedDriver);

        // Assert
        driver.FirstName.Should().Be(updatedDriver.FirstName);
        driver.LastName.Should().Be(updatedDriver.LastName);
    }
    
    [Fact]
    public void queue_domain_event_on_update()
    {
        // Arrange
        var driver = new FakeDriverBuilder().Build();
        var updatedDriver = new FakeDriverForUpdate().Generate();
        driver.DomainEvents.Clear();
        
        // Act
        driver.Update(updatedDriver);

        // Assert
        driver.DomainEvents.Count.Should().Be(1);
        driver.DomainEvents.FirstOrDefault().Should().BeOfType(typeof(DriverUpdated));
    }
}