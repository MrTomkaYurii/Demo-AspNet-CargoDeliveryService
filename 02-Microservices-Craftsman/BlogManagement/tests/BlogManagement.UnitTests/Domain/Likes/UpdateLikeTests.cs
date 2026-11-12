namespace BlogManagement.UnitTests.Domain.Likes;

using BlogManagement.SharedTestHelpers.Fakes.Like;
using BlogManagement.Domain.Likes;
using BlogManagement.Domain.Likes.DomainEvents;
using Bogus;
using FluentAssertions.Extensions;
using ValidationException = BlogManagement.Exceptions.ValidationException;

public class UpdateLikeTests
{
    private readonly Faker _faker;

    public UpdateLikeTests()
    {
        _faker = new Faker();
    }
    
    [Fact]
    public void can_update_like()
    {
        // Arrange
        var like = new FakeLikeBuilder().Build();
        var updatedLike = new FakeLikeForUpdate().Generate();
        
        // Act
        like.Update(updatedLike);

        // Assert
        like.Info.Should().Be(updatedLike.Info);
    }
    
    [Fact]
    public void queue_domain_event_on_update()
    {
        // Arrange
        var like = new FakeLikeBuilder().Build();
        var updatedLike = new FakeLikeForUpdate().Generate();
        like.DomainEvents.Clear();
        
        // Act
        like.Update(updatedLike);

        // Assert
        like.DomainEvents.Count.Should().Be(1);
        like.DomainEvents.FirstOrDefault().Should().BeOfType(typeof(LikeUpdated));
    }
}