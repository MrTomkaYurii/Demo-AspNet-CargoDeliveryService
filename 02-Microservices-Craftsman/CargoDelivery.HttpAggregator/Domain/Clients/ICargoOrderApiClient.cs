namespace CargoDelivery.HttpAggregator.Domain.Clients;

using CargoDelivery.HttpAggregator.Domain.Dtos;

/// <summary>
/// Інтерфейс типізованого HTTP-клієнта до мікросервісу OrderingManagement.
/// </summary>
public interface ICargoOrderApiClient
{
    Task<List<AggregatedCargoOrderDto>> GetActiveOrdersByDriverIdAsync(Guid driverId, CancellationToken cancellationToken = default);
}
