namespace CargoOrderingManager.FunctionalTests.FunctionalTests.Deliveries;

using CargoOrderingManager.SharedTestHelpers.Fakes.Delivery;
using CargoOrderingManager.FunctionalTests.TestUtilities;
using CargoOrderingManager.Domain;
using System.Net;
using System.Threading.Tasks;

public class GetDeliveryTests : TestBase
{
    [Fact]
    public async Task get_delivery_returns_success_when_entity_exists_using_valid_auth_credentials()
    {
        // Arrange
        var delivery = new FakeDeliveryBuilder().Build();

        var callingUser = await AddNewSuperAdmin();
        FactoryClient.AddAuth(callingUser.Identifier);
        await InsertAsync(delivery);

        // Act
        var route = ApiRoutes.Deliveries.GetRecord(delivery.Id);
        var result = await FactoryClient.GetRequestAsync(route);

        // Assert
        result.StatusCode.Should().Be(HttpStatusCode.OK);
    }
            
    [Fact]
    public async Task get_delivery_returns_unauthorized_without_valid_token()
    {
        // Arrange
        var delivery = new FakeDeliveryBuilder().Build();

        // Act
        var route = ApiRoutes.Deliveries.GetRecord(delivery.Id);
        var result = await FactoryClient.GetRequestAsync(route);

        // Assert
        result.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }
            
    [Fact]
    public async Task get_delivery_returns_forbidden_without_proper_scope()
    {
        // Arrange
        var delivery = new FakeDeliveryBuilder().Build();
        FactoryClient.AddAuth();

        // Act
        var route = ApiRoutes.Deliveries.GetRecord(delivery.Id);
        var result = await FactoryClient.GetRequestAsync(route);

        // Assert
        result.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }
}