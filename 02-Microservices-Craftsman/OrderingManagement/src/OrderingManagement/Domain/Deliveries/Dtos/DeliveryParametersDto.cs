namespace OrderingManagement.Domain.Deliveries.Dtos;

using OrderingManagement.Resources;

public sealed class DeliveryParametersDto : BasePaginationParameters
{
    public string? Filters { get; set; }
    public string? SortOrder { get; set; }
}
