// ====================================================================================
// 🧪 МОДУЛЬНІ ТЕСТИ РОУТИНГУ СХЕМ (SmartAuthPolicyExtensions)
// ------------------------------------------------------------------------------------
// Перевіряють, що PolicyScheme безпомилково розрізняє токени від усіх трьох модулів:
//   • 03.1 SimpleJwt  — iss == "AuthorizationModuleAPI"
//   • 03.2 Duende     — будь-який OIDC authority Duende (без "/realms/")
//   • 03.3 Keycloak   — OIDC authority з сегментом "/realms/DevRealmDrivers"
// ====================================================================================

using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using CargoDelivery.ServiceDefaults.Auth;
using FluentAssertions;
using Xunit;

namespace CargoDelivery.Auth.IntegrationTests;

public sealed class MultiSchemeSelectorTests
{
    [Fact]
    public void Should_Select_SimpleJwt_When_Issuer_Is_AuthorizationModuleAPI()
    {
        // Arrange: генеруємо JWT з issuer = "AuthorizationModuleAPI" (Модуль 03.1)
        var token = CreateUnsignedJwt(issuer: SimpleJwtAuthExtensions.DefaultIssuer);

        // Act
        var selectedScheme = SmartAuthPolicyExtensions.SelectSchemeFromHeader($"Bearer {token}");

        // Assert
        selectedScheme.Should().Be(AuthConstants.Schemes.SimpleJwt);
    }

    [Fact]
    public void Should_Select_Keycloak_When_Issuer_Contains_Realms_Segment()
    {
        // Arrange: генеруємо JWT з issuer Keycloak (Модуль 03.3)
        var token = CreateUnsignedJwt(issuer: "http://localhost:5080/auth/realms/DevRealmDrivers");

        // Act
        var selectedScheme = SmartAuthPolicyExtensions.SelectSchemeFromHeader($"Bearer {token}");

        // Assert
        selectedScheme.Should().Be(AuthConstants.Schemes.Keycloak);
    }

    [Fact]
    public void Should_Select_Duende_When_Issuer_Is_Duende_Authority()
    {
        // Arrange: генеруємо JWT з issuer Duende IdentityServer (Модуль 03.2 або федерація 03.3)
        var token = CreateUnsignedJwt(issuer: "https://localhost:5001");

        // Act
        var selectedScheme = SmartAuthPolicyExtensions.SelectSchemeFromHeader($"Bearer {token}");

        // Assert
        selectedScheme.Should().Be(AuthConstants.Schemes.Duende);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("Basic dXNlcjpwYXNz")]
    [InlineData("Bearer not-a-valid-jwt")]
    public void Should_Fallback_To_Duende_When_Header_Is_Missing_Or_Invalid(string? header)
    {
        // Act
        var selectedScheme = SmartAuthPolicyExtensions.SelectSchemeFromHeader(header);

        // Assert: безпечний fallback на стандартний OIDC challenge (401 Unauthorized)
        selectedScheme.Should().Be(AuthConstants.Schemes.Duende);
    }

    private static string CreateUnsignedJwt(string issuer)
    {
        var handler = new JwtSecurityTokenHandler();
        var token = new JwtSecurityToken(
            issuer: issuer,
            audience: "cargodelivery.microservices",
            claims: new[] { new Claim("sub", "1") },
            expires: DateTime.UtcNow.AddMinutes(10));

        return handler.WriteToken(token);
    }
}
