namespace CargoOrderingManager.Domain.Orders.Dtos;

using CargoOrderingManager.Resources;

public sealed class OrderParametersDto : BasePaginationParameters
{
    public string? Filters { get; set; }
    public string? SortOrder { get; set; }
}
