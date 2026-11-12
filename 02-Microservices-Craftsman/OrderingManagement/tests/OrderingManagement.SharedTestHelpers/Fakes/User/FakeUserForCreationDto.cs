namespace OrderingManagement.SharedTestHelpers.Fakes.User;

using AutoBogus;
using OrderingManagement.Domain;
using OrderingManagement.Domain.Users.Dtos;
using OrderingManagement.Domain.Roles;
using OrderingManagement.Domain.Users.Models;

public sealed class FakeUserForCreationDto : AutoFaker<UserForCreationDto>
{
    public FakeUserForCreationDto()
    {
        RuleFor(u => u.Email, f => f.Person.Email);
    }
}