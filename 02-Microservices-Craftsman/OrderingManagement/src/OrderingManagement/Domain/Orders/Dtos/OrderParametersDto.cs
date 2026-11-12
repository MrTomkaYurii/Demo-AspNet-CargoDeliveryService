namespace OrderingManagement.Domain.Orders.Dtos;

using OrderingManagement.Resources;

public sealed class OrderParametersDto : BasePaginationParameters
{
    public string? Filters { get; set; }
    public string? SortOrder { get; set; }
}
