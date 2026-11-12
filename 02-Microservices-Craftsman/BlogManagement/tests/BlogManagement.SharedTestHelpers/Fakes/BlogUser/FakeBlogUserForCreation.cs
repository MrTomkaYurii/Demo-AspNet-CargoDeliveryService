namespace BlogManagement.SharedTestHelpers.Fakes.BlogUser;

using AutoBogus;
using BlogManagement.Domain.BlogUsers;
using BlogManagement.Domain.BlogUsers.Models;

public sealed class FakeBlogUserForCreation : AutoFaker<BlogUserForCreation>
{
    public FakeBlogUserForCreation()
    {
    }
}