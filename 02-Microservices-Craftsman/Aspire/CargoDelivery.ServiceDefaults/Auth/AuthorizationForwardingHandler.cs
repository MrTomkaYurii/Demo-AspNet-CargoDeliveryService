// ====================================================================================
// 🔁 НАСКРІЗНЕ ПРОКИДАННЯ ТОКЕНА (TOKEN PROPAGATION) ДЛЯ HTTP-АГРЕГАТОРА
// ------------------------------------------------------------------------------------
// Використовується у CargoDelivery.HttpAggregator, щоб кожен вихідний запит до
// внутрішніх мікросервісів (DriversManagement, BlogManagement, OrderingManagement)
// зберігав заголовок Authorization: Bearer ... від початкового клієнта.
// ====================================================================================

using System.Net.Http.Headers;
using Microsoft.AspNetCore.Http;

namespace CargoDelivery.ServiceDefaults.Auth;

/// <summary>
/// DelegatingHandler для копіювання заголовка Authorization із вхідного HttpContext
/// у вихідний HttpRequestMessage.
/// </summary>
public sealed class AuthorizationForwardingHandler : DelegatingHandler
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public AuthorizationForwardingHandler(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    protected override Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request,
        CancellationToken cancellationToken)
    {
        var authHeader = _httpContextAccessor.HttpContext?.Request.Headers.Authorization.FirstOrDefault();
        if (!string.IsNullOrWhiteSpace(authHeader) &&
            AuthenticationHeaderValue.TryParse(authHeader, out var parsedHeader))
        {
            request.Headers.Authorization = parsedHeader;
        }

        return base.SendAsync(request, cancellationToken);
    }
}
