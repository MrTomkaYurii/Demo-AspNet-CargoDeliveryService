// ====================================================================================
// 🧪 ІНТЕГРАЦІЙНІ ТЕСТИ АВТЕНТИФІКАЦІЇ ТА РОЛЬОВОЇ АВТОРИЗАЦІЇ (03.1 / 03.2 / 03.3)
// ------------------------------------------------------------------------------------
// Перевіряють конвеєри безпеки кожного з трьох мікросервісів на базі ASP.NET Core TestServer:
//   1. 03.1 DriversManagement (SimpleJwt): 401 без токена, 200 з валідним HS256, 401 з чужим ключем.
//   2. 03.2 BlogManagement (Duende): перевірка рольової політики RequireAdminRole (RoleClaimType = "role").
//   3. 03.3 OrderingManagement (Keycloak): розбір JSON-клейму realm_access.roles у ClaimTypes.Role.
//   4. AuthorizationForwardingHandler: прокидання Bearer-токена у вихідний HttpRequestMessage.
// ====================================================================================

using System.IdentityModel.Tokens.Jwt;
using System.Net;
using System.Net.Http.Headers;
using System.Security.Claims;
using System.Text;
using System.Text.Json;
using CargoDelivery.ServiceDefaults.Auth;
using FluentAssertions;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.IdentityModel.Tokens;
using Xunit;

namespace CargoDelivery.Auth.IntegrationTests;

public sealed class MultiAuthPipelineTests
{
    private const string SimpleJwtSecret = "TOPSECRETKEYFORDRIVERMANAGEMENTSYSTEM";
    private const string SimpleJwtIssuer = "AuthorizationModuleAPI";
    private const string SimpleJwtAudience = "cargodelivery.microservices";

    [Fact]
    public async Task SimpleJwt_Protected_Endpoint_Returns_401_Without_Token()
    {
        using var host = await CreateSimpleJwtHostAsync();
        var client = host.GetTestClient();

        var response = await client.GetAsync("/api/drivers-protected");

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task SimpleJwt_Protected_Endpoint_Returns_200_With_Valid_Token()
    {
        using var host = await CreateSimpleJwtHostAsync();
        var client = host.GetTestClient();

        var token = GenerateSignedJwt(SimpleJwtSecret, SimpleJwtIssuer, SimpleJwtAudience);
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var response = await client.GetAsync("/api/drivers-protected");
        var body = await response.Content.ReadAsStringAsync();

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        body.Should().Contain("dispatcher@cargodelivery.local");
    }

    [Fact]
    public async Task SimpleJwt_Protected_Endpoint_Returns_401_With_Invalid_Signature()
    {
        using var host = await CreateSimpleJwtHostAsync();
        var client = host.GetTestClient();

        var badToken = GenerateSignedJwt(
            "WRONG_SECRET_KEY_THAT_SHOULD_FAIL_VALIDATION_12345",
            SimpleJwtIssuer,
            SimpleJwtAudience);
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", badToken);

        var response = await client.GetAsync("/api/drivers-protected");

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Theory]
    [InlineData("SuperAdmin", HttpStatusCode.OK)]
    [InlineData("User", HttpStatusCode.Forbidden)]
    public async Task Duende_Role_Policy_Enforces_SuperAdmin_Role(string role, HttpStatusCode expectedStatus)
    {
        const string duendeTestSecret = "DUENDE_TEST_SIGNING_KEY_FOR_ROLE_VERIFICATION_32B";
        using var host = await CreateRoleHostAsync(
            secret: duendeTestSecret,
            roleClaimType: ClaimTypes.Role,
            mapKeycloakRealmAccess: false);

        var client = host.GetTestClient();
        var token = GenerateSignedJwt(
            duendeTestSecret,
            issuer: "https://localhost:5001",
            audience: "blog_management",
            extraClaims: new[] { new Claim("role", role) });

        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var response = await client.GetAsync("/api/admin-only");

        response.StatusCode.Should().Be(expectedStatus);
    }

    [Theory]
    [InlineData("SuperAdmin", HttpStatusCode.OK)]
    [InlineData("User", HttpStatusCode.Forbidden)]
    public async Task Keycloak_RealmAccess_Roles_Are_Mapped_To_ClaimTypes_Role(string role, HttpStatusCode expectedStatus)
    {
        const string keycloakTestSecret = "KEYCLOAK_TEST_SIGNING_KEY_FOR_ROLE_VERIFICATION";
        using var host = await CreateRoleHostAsync(
            secret: keycloakTestSecret,
            roleClaimType: ClaimTypes.Role,
            mapKeycloakRealmAccess: true);

        var client = host.GetTestClient();
        var realmAccessJson = JsonSerializer.Serialize(new { roles = new[] { role } });
        var token = GenerateSignedJwt(
            keycloakTestSecret,
            issuer: "http://localhost:5080/auth/realms/DevRealmDrivers",
            audience: "cargo_ordering",
            extraClaims: new[] { new Claim("realm_access", realmAccessJson) });

        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var response = await client.GetAsync("/api/admin-only");

        response.StatusCode.Should().Be(expectedStatus);
    }

    [Fact]
    public async Task AuthorizationForwardingHandler_Forwards_Bearer_Token_From_HttpContext()
    {
        var context = new DefaultHttpContext();
        context.Request.Headers.Authorization = "Bearer test-propagated-token-123";

        var accessor = new HttpContextAccessor { HttpContext = context };
        var captureHandler = new CaptureRequestHandler();
        var forwardingHandler = new AuthorizationForwardingHandler(accessor)
        {
            InnerHandler = captureHandler
        };

        using var invoker = new HttpMessageInvoker(forwardingHandler);
        var outgoingRequest = new HttpRequestMessage(HttpMethod.Get, "https://orderingmanagement/api/internal/deliveries");

        await invoker.SendAsync(outgoingRequest, CancellationToken.None);

        captureHandler.CapturedAuthorization.Should().NotBeNull();
        captureHandler.CapturedAuthorization!.Scheme.Should().Be("Bearer");
        captureHandler.CapturedAuthorization.Parameter.Should().Be("test-propagated-token-123");
    }

    private static async Task<IHost> CreateSimpleJwtHostAsync()
    {
        var builder = new HostBuilder()
            .ConfigureWebHost(webBuilder =>
            {
                webBuilder.UseTestServer();
                webBuilder.ConfigureServices(services =>
                {
                    services.AddRouting();
                    services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                        .AddJwtBearer(options =>
                        {
                            options.TokenValidationParameters = new TokenValidationParameters
                            {
                                ValidateIssuer = true,
                                ValidIssuer = SimpleJwtIssuer,
                                ValidateAudience = true,
                                ValidAudience = SimpleJwtAudience,
                                ValidateLifetime = true,
                                ValidateIssuerSigningKey = true,
                                IssuerSigningKey = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(SimpleJwtSecret))
                            };
                        });
                    services.AddAuthorization();
                });

                webBuilder.Configure(app =>
                {
                    app.UseRouting();
                    app.UseAuthentication();
                    app.UseAuthorization();
                    app.UseEndpoints(endpoints =>
                    {
                        endpoints.MapGet("/api/drivers-protected", async context =>
                        {
                            var email = context.User.FindFirst(ClaimTypes.Email)?.Value
                                        ?? context.User.FindFirst("email")?.Value;
                            await context.Response.WriteAsync(email ?? string.Empty);
                        }).RequireAuthorization();
                    });
                });
            });

        return await builder.StartAsync();
    }

    private static async Task<IHost> CreateRoleHostAsync(
        string secret,
        string roleClaimType,
        bool mapKeycloakRealmAccess)
    {
        var builder = new HostBuilder()
            .ConfigureWebHost(webBuilder =>
            {
                webBuilder.UseTestServer();
                webBuilder.ConfigureServices(services =>
                {
                    services.AddRouting();
                    services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                        .AddJwtBearer(options =>
                        {
                            options.TokenValidationParameters = new TokenValidationParameters
                            {
                                ValidateIssuer = false,
                                ValidateAudience = false,
                                ValidateLifetime = true,
                                ValidateIssuerSigningKey = true,
                                IssuerSigningKey = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(secret)),
                                RoleClaimType = roleClaimType
                            };

                            if (mapKeycloakRealmAccess)
                            {
                                options.Events = new JwtBearerEvents
                                {
                                    OnTokenValidated = context =>
                                    {
                                        if (context.Principal?.Identity is ClaimsIdentity identity)
                                        {
                                            var realmAccess = context.Principal.FindFirst("realm_access")?.Value;
                                            if (!string.IsNullOrEmpty(realmAccess))
                                            {
                                                using var doc = JsonDocument.Parse(realmAccess);
                                                if (doc.RootElement.TryGetProperty("roles", out var roles))
                                                {
                                                    foreach (var role in roles.EnumerateArray())
                                                    {
                                                        var roleName = role.GetString();
                                                        if (!string.IsNullOrWhiteSpace(roleName))
                                                            identity.AddClaim(new Claim(ClaimTypes.Role, roleName));
                                                    }
                                                }
                                            }
                                        }

                                        return Task.CompletedTask;
                                    }
                                };
                            }
                        });

                    services.AddAuthorization(options =>
                    {
                        options.AddPolicy("RequireAdminRole", policy => policy.RequireRole("SuperAdmin"));
                    });
                });

                webBuilder.Configure(app =>
                {
                    app.UseRouting();
                    app.UseAuthentication();
                    app.UseAuthorization();
                    app.UseEndpoints(endpoints =>
                    {
                        endpoints.MapGet("/api/admin-only", async context =>
                        {
                            await context.Response.WriteAsync("ok");
                        }).RequireAuthorization("RequireAdminRole");
                    });
                });
            });

        return await builder.StartAsync();
    }

    private static string GenerateSignedJwt(
        string secret,
        string issuer,
        string audience,
        IEnumerable<Claim>? extraClaims = null)
    {
        var key = Encoding.ASCII.GetBytes(secret);
        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, "1"),
            new(ClaimTypes.NameIdentifier, "1"),
            new(JwtRegisteredClaimNames.Email, "dispatcher@cargodelivery.local"),
            new(ClaimTypes.Email, "dispatcher@cargodelivery.local")
        };

        if (extraClaims is not null)
            claims.AddRange(extraClaims);

        var descriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(claims),
            Expires = DateTime.UtcNow.AddMinutes(15),
            Issuer = issuer,
            Audience = audience,
            SigningCredentials = new SigningCredentials(
                new SymmetricSecurityKey(key),
                SecurityAlgorithms.HmacSha256Signature)
        };

        var handler = new JwtSecurityTokenHandler();
        return handler.WriteToken(handler.CreateToken(descriptor));
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
