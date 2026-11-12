namespace BlogManagement.SharedTestHelpers.Fakes.BlogUser;

using BlogManagement.Domain.BlogUsers;
using BlogManagement.Domain.BlogUsers.Models;

public class FakeBlogUserBuilder
{
    private BlogUserForCreation _creationData = new FakeBlogUserForCreation().Generate();

    public FakeBlogUserBuilder WithModel(BlogUserForCreation model)
    {
        _creationData = model;
        return this;
    }
    
    public FakeBlogUserBuilder WithUserName(string userName)
    {
        _creationData.UserName = userName;
        return this;
    }
    
    public FakeBlogUserBuilder WithEmail(string email)
    {
        _creationData.Email = email;
        return this;
    }
    
    public BlogUser Build()
    {
        var result = BlogUser.Create(_creationData);
        return result;
    }
}