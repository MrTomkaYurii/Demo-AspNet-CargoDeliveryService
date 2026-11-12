namespace BlogManagement.SharedTestHelpers.Fakes.Comment;

using AutoBogus;
using BlogManagement.Domain.Comments;
using BlogManagement.Domain.Comments.Models;

public sealed class FakeCommentForCreation : AutoFaker<CommentForCreation>
{
    public FakeCommentForCreation()
    {
    }
}