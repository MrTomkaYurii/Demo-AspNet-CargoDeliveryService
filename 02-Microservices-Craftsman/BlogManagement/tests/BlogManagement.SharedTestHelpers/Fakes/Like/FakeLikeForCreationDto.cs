namespace BlogManagement.SharedTestHelpers.Fakes.Like;

using AutoBogus;
using BlogManagement.Domain.Likes;
using BlogManagement.Domain.Likes.Dtos;

public sealed class FakeLikeForCreationDto : AutoFaker<LikeForCreationDto>
{
    public FakeLikeForCreationDto()
    {
    }
}