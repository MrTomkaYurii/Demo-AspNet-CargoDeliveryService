namespace BlogManagement.SharedTestHelpers.Fakes.BlogUser;

using AutoBogus;
using BlogManagement.Domain.BlogUsers;
using BlogManagement.Domain.BlogUsers.Dtos;

public sealed class FakeBlogUserForUpdateDto : AutoFaker<BlogUserForUpdateDto>
{
    public FakeBlogUserForUpdateDto()
    {
    }
}