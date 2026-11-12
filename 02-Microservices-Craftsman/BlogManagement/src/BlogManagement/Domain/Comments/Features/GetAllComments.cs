namespace BlogManagement.Domain.Comments.Features;

using BlogManagement.Domain.Comments.Dtos;
using BlogManagement.Domain.Comments.Services;
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

public static class GetAllComments
{
    public sealed record Query() : IRequest<List<CommentDto>>;

    public sealed class Handler : IRequestHandler<Query, List<CommentDto>>
    {
        private readonly ICommentRepository _commentRepository;
        private readonly IHeimGuardClient _heimGuard;

        public Handler(ICommentRepository commentRepository, IHeimGuardClient heimGuard)
        {
            _commentRepository = commentRepository;
            _heimGuard = heimGuard;
        }

        public async Task<List<CommentDto>> Handle(Query request, CancellationToken cancellationToken)
        {
            // [AUTH-DISABLED] await _heimGuard.MustHavePermission<ForbiddenAccessException>(Permissions.CanReadComments);

            return _commentRepository.Query()
                .AsNoTracking()
                .ToCommentDtoQueryable()
                .ToList();
        }
    }
}