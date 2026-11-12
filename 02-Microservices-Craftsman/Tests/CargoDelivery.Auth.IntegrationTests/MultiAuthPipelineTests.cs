// ====================================================================================
// 🧪 ІНТЕГРАЦІЙНІ ТЕСТИ КОНВЕЄРА АВТЕНТИФІКАЦІЇ ТА ПРОКИДАННЯ ТОКЕНА
// ------------------------------------------------------------------------------------
// Використовують ASP.NET Core TestServer із реальним AddCargoMultiAuth(...):
//   1. Запит без токена до захищеного ендпоінта повертає 401 Unauthorized.
//   2. Валідний токен 03.1 (SimpleJwt HMAC-SHA256) проходить SmartPolicySelector -> 200 OK.
//   3. Токен з неправильним підписом відхиляється -> 401 Unauthorized.
//   4. AuthorizationForwardingHandler коректно копіює Bearer-токен у вихідний HttpRequestMessage.
// ====================================================================================

using System.IdentityModel.Tokens.Jwt;
using System.Net;
using System.Net.Http.Headers;
using System.Security.Claims;
using System.Text;
using CargoDelivery.ServiceDefaults.Auth;
using FluentAssertions;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.IdentityModel.Tokens;
using Xunit;

namespace CargoDelivery.Auth.IntegrationTests;

public sealed class MultiAuthPipelineTests
{
    [Fact]
    public async Task Protected_Endpoint_Returns_401_Without_Token()
    {
        // Arrange
        using var host = await CreateTestHostAsync();
        var client = host.GetTestClient();

        // Act
        var response = await client.GetAsync("/api/test-protected");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Protected_Endpoint_Returns_200_With_Valid_SimpleJwt_Token()
    {
        // Arrange: виписуємо реальний підписаний токен за правилами AuthorizationModuleAPI (03.1)
        using var host = await CreateTestHostAsync();
        var client = host.GetTestClient();

        var token = GenerateSignedSimpleJwt(
            secret: SimpleJwtAuthExtensions.DefaultSecret,
            issuer: SimpleJwtAuthExtensions.DefaultIssuer,
            audience: SimpleJwtAuthExtensions.DefaultAudience);

        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        // Act
        var response = await client.GetAsync("/api/test-protected");
        var body = await response.Content.ReadAsStringAsync();

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        body.Should().Contain("dispatcher@cargodelivery.local");
    }

    [Fact]
    public async Task Protected_Endpoint_Returns_401_With_Invalid_Signature_Token()
    {
        // Arrange
        using var host = await CreateTestHostAsync();
        var client = host.GetTestClient();

        var badToken = GenerateSignedSimpleJwt(
            secret: "WRONG_SECRET_KEY_THAT_SHOULD_FAIL_VALIDATION_12345",
            issuer: SimpleJwtAuthExtensions.DefaultIssuer,
            audience: SimpleJwtAuthExtensions.DefaultAudience);

        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", badToken);

        // Act
        var response = await client.GetAsync("/api/test-protected");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task AuthorizationForwardingHandler_Propagates_Bearer_Token_Downstream()
    {
        // Arrange: імітуємо вхідний HttpContext із заголовком Authorization (як у HttpAggregator)
        var httpContext = new DefaultHttpContext();
        httpContext.Request.Headers.Authorization = "Bearer demo-propagated-token";

        var accessor = new HttpContextAccessor { HttpContext = httpContext };
        var captureHandler = new CaptureRequestHandler();
        var forwardingHandler = new AuthorizationForwardingHandler(accessor)
        {
            InnerHandler = captureHandler
        };

        using var invoker = new HttpMessageInvoker(forwardingHandler);
        using var request = new HttpRequestMessage(HttpMethod.Get, "https://driversmanagement/api/drivers");

        // Act
        await invoker.SendAsync(request, CancellationToken.None);

        // Assert
        captureHandler.CapturedAuthorization.Should().NotBeNull();
        captureHandler.CapturedAuthorization!.Scheme.Should().Be("Bearer");
        captureHandler.CapturedAuthorization!.Parameter.Should().Be("demo-propagated-token");
    }

    private static async Task<IHost> CreateTestHostAsync()
    {
        var configValues = new Dictionary<string, string?>
        {
            ["Auth:Mode"] = AuthConstants.Modes.Multi,
            ["Auth:SimpleJwt:Secret"] = SimpleJwtAuthExtensions.DefaultSecret,
            ["Auth:SimpleJwt:Issuer"] = SimpleJwtAuthExtensions.DefaultIssuer,
            ["Auth:SimpleJwt:Audience"] = SimpleJwtAuthExtensions.DefaultAudience,
            ["Auth:Duende:Authority"] = "https://localhost:5001",
            ["Auth:Keycloak:Authority"] = "http://localhost:5080/auth/realms/DevRealmDrivers"
        };

        var host = new HostBuilder()
            .ConfigureWebHost(webBuilder =>
            {
                webBuilder.UseTestServer();
                webBuilder.ConfigureAppConfiguration((_, config) => config.AddInMemoryCollection(configValues));
                webBuilder.ConfigureServices((ctx, services) =>
                {
                    services.AddRouting();
                    services.AddAuthorization();
                    services.AddCargoMultiAuth(ctx.Configuration, ctx.HostingEnvironment);
                });
                webBuilder.Configure(app =>
                {
                    app.UseRouting();
                    app.UseAuthentication();
                    app.UseAuthorization();
                    app.UseEndpoints(endpoints =>
                    {
                        endpoints.MapGet("/api/test-protected", (ClaimsPrincipal user) =>
                        {
                            var email = user.FindFirstValue(ClaimTypes.Email) ?? "unknown";
                            return Results.Text($"Authenticated:{email}");
                        }).RequireAuthorization();
                    });
                });
            })
            .Build();

        await host.StartAsync();
        return host;
    }

    private static string GenerateSignedSimpleJwt(string secret, string issuer, string audience)
    {
        var key = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(secret));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256Signature);

        var token = new JwtSecurityToken(
            issuer: issuer,
            audience: audience,
            claims: new[]
            {
                new Claim(ClaimTypes.NameIdentifier, "42"),
                new Claim(ClaimTypes.Email, "dispatcher@cargodelivery.local"),
                new Claim(ClaimTypes.Role, "SuperAdmin")
            },
            expires: DateTime.UtcNow.AddMinutes(15),
            signingCredentials: creds);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    private sealed class CaptureRequestHandler : HttpMessageHandler
    {
        public AuthenticationHeaderValue? CapturedAuthorization { get; private set; }

        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            CapturedAuthorization = request.Headers.Authorization;
            return Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK));
        }
    }
}
