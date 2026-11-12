namespace BlogManagement.SharedTestHelpers.Fakes.Driver;

using AutoBogus;
using BlogManagement.Domain.Drivers;
using BlogManagement.Domain.Drivers.Models;

public sealed class FakeDriverForCreation : AutoFaker<DriverForCreation>
{
    public FakeDriverForCreation()
    {
    }
}