namespace BlogManagement.SharedTestHelpers.Fakes.PostAboutDriver;

using AutoBogus;
using BlogManagement.Domain.PostAboutDrivers;
using BlogManagement.Domain.PostAboutDrivers.Dtos;

public sealed class FakePostAboutDriverForUpdateDto : AutoFaker<PostAboutDriverForUpdateDto>
{
    public FakePostAboutDriverForUpdateDto()
    {
    }
}