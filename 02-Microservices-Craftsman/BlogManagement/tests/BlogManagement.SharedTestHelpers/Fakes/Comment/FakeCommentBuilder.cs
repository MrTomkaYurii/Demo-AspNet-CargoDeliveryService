namespace BlogManagement.SharedTestHelpers.Fakes.Comment;

using BlogManagement.Domain.Comments;
using BlogManagement.Domain.Comments.Models;

public class FakeCommentBuilder
{
    private CommentForCreation _creationData = new FakeCommentForCreation().Generate();

    public FakeCommentBuilder WithModel(CommentForCreation model)
    {
        _creationData = model;
        return this;
    }
    
    public FakeCommentBuilder WithText(string text)
    {
        _creationData.Text = text;
        return this;
    }
    
    public Comment Build()
    {
        var result = Comment.Create(_creationData);
        return result;
    }
}