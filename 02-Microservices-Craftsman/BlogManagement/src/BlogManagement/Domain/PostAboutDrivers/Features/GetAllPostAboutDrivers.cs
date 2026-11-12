namespace BlogManagement.Domain.PostAboutDrivers.Features;

using BlogManagement.Domain.PostAboutDrivers.Dtos;
using BlogManagement.Domain.PostAboutDrivers.Services;
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

public static class GetAllPostAboutDrivers
{
    public sealed record Query() : IRequest<List<PostAboutDriverDto>>;

    public sealed class Handler : IRequestHandler<Query, List<PostAboutDriverDto>>
    {
        private readonly IPostAboutDriverRepository _postAboutDriverRepository;
        private readonly IHeimGuardClient _heimGuard;

        public Handler(IPostAboutDriverRepository postAboutDriverRepository, IHeimGuardClient heimGuard)
        {
            _postAboutDriverRepository = postAboutDriverRepository;
            _heimGuard = heimGuard;
        }

        public async Task<List<PostAboutDriverDto>> Handle(Query request, CancellationToken cancellationToken)
        {
            // [AUTH-DISABLED] await _heimGuard.MustHavePermission<ForbiddenAccessException>(Permissions.CanReadPostAboutDrivers);

            return _postAboutDriverRepository.Query()
                .AsNoTracking()
                .ToPostAboutDriverDtoQueryable()
                .ToList();
        }
    }
}