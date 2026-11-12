namespace BlogManagement.SharedTestHelpers.Fakes.User;

using AutoBogus;
using BlogManagement.Domain;
using BlogManagement.Domain.Users.Dtos;
using BlogManagement.Domain.Roles;
using BlogManagement.Domain.Users.Models;

public sealed class FakeUserForCreation : AutoFaker<UserForCreation>
{
    public FakeUserForCreation()
    {
        RuleFor(u => u.Email, f => f.Person.Email);
    }
}