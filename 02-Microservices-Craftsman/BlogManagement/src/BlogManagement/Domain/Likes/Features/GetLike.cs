namespace BlogManagement.Domain.Likes.Features;

using BlogManagement.Domain.Likes.Dtos;
using BlogManagement.Domain.Likes.Services;
using BlogManagement.Exceptions;
using BlogManagement.Domain;
using HeimGuard;
using Mappings;
using MediatR;

public static class GetLike
{
    public sealed record Query(Guid LikeId) : IRequest<LikeDto>;

    public sealed class Handler : IRequestHandler<Query, LikeDto>
    {
        private readonly ILikeRepository _likeRepository;
        private readonly IHeimGuardClient _heimGuard;

        public Handler(ILikeRepository likeRepository, IHeimGuardClient heimGuard)
        {
            _likeRepository = likeRepository;
            _heimGuard = heimGuard;
        }

        public async Task<LikeDto> Handle(Query request, CancellationToken cancellationToken)
        {
            // [AUTH-DISABLED] await _heimGuard.MustHavePermission<ForbiddenAccessException>(Permissions.CanReadLike);

            var result = await _likeRepository.GetById(request.LikeId, cancellationToken: cancellationToken);
            return result.ToLikeDto();
        }
    }
}