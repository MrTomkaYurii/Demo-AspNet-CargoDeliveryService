namespace BlogManagement.SharedTestHelpers.Fakes.Driver;

using AutoBogus;
using BlogManagement.Domain.Drivers;
using BlogManagement.Domain.Drivers.Dtos;

public sealed class FakeDriverForUpdateDto : AutoFaker<DriverForUpdateDto>
{
    public FakeDriverForUpdateDto()
    {
    }
}