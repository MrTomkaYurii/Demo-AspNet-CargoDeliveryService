namespace OrderingManagement.Domain.Users.Dtos;

using OrderingManagement.Resources;

public sealed class UserParametersDto : BasePaginationParameters
{
    public string? Filters { get; set; }
    public string? SortOrder { get; set; }
}
