namespace BlogManagement.SharedTestHelpers.Fakes.Like;

using BlogManagement.Domain.Likes;
using BlogManagement.Domain.Likes.Models;

public class FakeLikeBuilder
{
    private LikeForCreation _creationData = new FakeLikeForCreation().Generate();

    public FakeLikeBuilder WithModel(LikeForCreation model)
    {
        _creationData = model;
        return this;
    }
    
    public FakeLikeBuilder WithInfo(string info)
    {
        _creationData.Info = info;
        return this;
    }
    
    public Like Build()
    {
        var result = Like.Create(_creationData);
        return result;
    }
}