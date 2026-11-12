namespace BlogManagement.Domain.PostAboutDrivers.Mappings;

using BlogManagement.Domain.PostAboutDrivers.Dtos;
using BlogManagement.Domain.PostAboutDrivers.Models;
using Riok.Mapperly.Abstractions;

[Mapper]
public static partial class PostAboutDriverMapper
{
    public static partial PostAboutDriverForCreation ToPostAboutDriverForCreation(this PostAboutDriverForCreationDto postAboutDriverForCreationDto);
    public static partial PostAboutDriverForUpdate ToPostAboutDriverForUpdate(this PostAboutDriverForUpdateDto postAboutDriverForUpdateDto);
    public static partial PostAboutDriverDto ToPostAboutDriverDto(this PostAboutDriver postAboutDriver);
    public static partial IQueryable<PostAboutDriverDto> ToPostAboutDriverDtoQueryable(this IQueryable<PostAboutDriver> postAboutDriver);
}