namespace BlogManagement.SharedTestHelpers.Fakes.Comment;

using AutoBogus;
using BlogManagement.Domain.Comments;
using BlogManagement.Domain.Comments.Dtos;

public sealed class FakeCommentForUpdateDto : AutoFaker<CommentForUpdateDto>
{
    public FakeCommentForUpdateDto()
    {
    }
}