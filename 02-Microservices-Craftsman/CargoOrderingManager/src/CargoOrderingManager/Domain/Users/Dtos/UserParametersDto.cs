namespace CargoOrderingManager.Domain.Users.Dtos;

using CargoOrderingManager.Resources;

public sealed class UserParametersDto : BasePaginationParameters
{
    public string? Filters { get; set; }
    public string? SortOrder { get; set; }
}
