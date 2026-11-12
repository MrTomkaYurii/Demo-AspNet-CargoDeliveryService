namespace BlogManagement.Domain.Likes.Features;

using BlogManagement.Domain.Likes.Dtos;
using BlogManagement.Domain.Likes.Services;
using BlogManagement.Wrappers;
using BlogManagement.Exceptions;
using BlogManagement.Resources;
using BlogManagement.Domain;
using HeimGuard;
using Mappings;
using Microsoft.EntityFrameworkCore;
using MediatR;
using QueryKit;
using QueryKit.Configuration;

public static class GetAllLikes
{
    public sealed record Query() : IRequest<List<LikeDto>>;

    public sealed class Handler : IRequestHandler<Query, List<LikeDto>>
    {
        private readonly ILikeRepository _likeRepository;
        private readonly IHeimGuardClient _heimGuard;

        public Handler(ILikeRepository likeRepository, IHeimGuardClient heimGuard)
        {
            _likeRepository = likeRepository;
            _heimGuard = heimGuard;
        }

        public async Task<List<LikeDto>> Handle(Query request, CancellationToken cancellationToken)
        {
            // [AUTH-DISABLED] await _heimGuard.MustHavePermission<ForbiddenAccessException>(Permissions.CanReadLikes);

            return _likeRepository.Query()
                .AsNoTracking()
                .ToLikeDtoQueryable()
                .ToList();
        }
    }
}