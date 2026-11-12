namespace BlogManagement.Domain.BlogUsers.Features;

using BlogManagement.Domain.BlogUsers.Dtos;
using BlogManagement.Domain.BlogUsers.Services;
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

public static class GetAllBlogUsers
{
    public sealed record Query() : IRequest<List<BlogUserDto>>;

    public sealed class Handler : IRequestHandler<Query, List<BlogUserDto>>
    {
        private readonly IBlogUserRepository _blogUserRepository;
        private readonly IHeimGuardClient _heimGuard;

        public Handler(IBlogUserRepository blogUserRepository, IHeimGuardClient heimGuard)
        {
            _blogUserRepository = blogUserRepository;
            _heimGuard = heimGuard;
        }

        public async Task<List<BlogUserDto>> Handle(Query request, CancellationToken cancellationToken)
        {
            // [AUTH-DISABLED] await _heimGuard.MustHavePermission<ForbiddenAccessException>(Permissions.CanReadUsers);

            return _blogUserRepository.Query()
                .AsNoTracking()
                .ToBlogUserDtoQueryable()
                .ToList();
        }
    }
}