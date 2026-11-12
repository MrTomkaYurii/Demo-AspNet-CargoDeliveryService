namespace BlogManagement.SharedTestHelpers.Fakes.Like;

using AutoBogus;
using BlogManagement.Domain.Likes;
using BlogManagement.Domain.Likes.Models;

public sealed class FakeLikeForCreation : AutoFaker<LikeForCreation>
{
    public FakeLikeForCreation()
    {
    }
}