namespace BlogManagement.UnitTests.Domain.Comments;

using BlogManagement.SharedTestHelpers.Fakes.Comment;
using BlogManagement.Domain.Comments;
using BlogManagement.Domain.Comments.DomainEvents;
using Bogus;
using FluentAssertions.Extensions;
using ValidationException = BlogManagement.Exceptions.ValidationException;

public class CreateCommentTests
{
    private readonly Faker _faker;

    public CreateCommentTests()
    {
        _faker = new Faker();
    }
    
    [Fact]
    public void can_create_valid_comment()
    {
        // Arrange
        var commentToCreate = new FakeCommentForCreation().Generate();
        
        // Act
        var comment = Comment.Create(commentToCreate);

        // Assert
        comment.Text.Should().Be(commentToCreate.Text);
    }

    [Fact]
    public void queue_domain_event_on_create()
    {
        // Arrange
        var commentToCreate = new FakeCommentForCreation().Generate();
        
        // Act
        var comment = Comment.Create(commentToCreate);

        // Assert
        comment.DomainEvents.Count.Should().Be(1);
        comment.DomainEvents.FirstOrDefault().Should().BeOfType(typeof(CommentCreated));
    }
}