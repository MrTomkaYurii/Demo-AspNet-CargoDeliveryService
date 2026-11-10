namespace CargoOrderingManager.SharedTestHelpers.Fakes.User;

using AutoBogus;
using CargoOrderingManager.Domain;
using CargoOrderingManager.Domain.Users.Dtos;
using CargoOrderingManager.Domain.Roles;
using CargoOrderingManager.Domain.Users.Models;

public sealed class FakeUserForUpdateDto : AutoFaker<UserForUpdateDto>
{
    public FakeUserForUpdateDto()
    {
        RuleFor(u => u.Email, f => f.Person.Email);
    }
}