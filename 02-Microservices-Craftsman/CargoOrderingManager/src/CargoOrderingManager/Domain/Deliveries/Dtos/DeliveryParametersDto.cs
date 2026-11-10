namespace CargoOrderingManager.Domain.Deliveries.Dtos;

using CargoOrderingManager.Resources;

public sealed class DeliveryParametersDto : BasePaginationParameters
{
    public string? Filters { get; set; }
    public string? SortOrder { get; set; }
}
