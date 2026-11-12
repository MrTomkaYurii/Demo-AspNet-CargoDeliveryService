namespace BlogManagement.Domain.Comments.Features;

using BlogManagement.Domain.Comments.Dtos;
using BlogManagement.Domain.Comments.Services;
using BlogManagement.Exceptions;
using BlogManagement.Domain;
using HeimGuard;
using Mappings;
using MediatR;

public static class GetComment
{
    public sealed record Query(Guid CommentId) : IRequest<CommentDto>;

    public sealed class Handler : IRequestHandler<Query, CommentDto>
    {
        private readonly ICommentRepository _commentRepository;
        private readonly IHeimGuardClient _heimGuard;

        public Handler(ICommentRepository commentRepository, IHeimGuardClient heimGuard)
        {
            _commentRepository = commentRepository;
            _heimGuard = heimGuard;
        }

        public async Task<CommentDto> Handle(Query request, CancellationToken cancellationToken)
        {
            // [AUTH-DISABLED] await _heimGuard.MustHavePermission<ForbiddenAccessException>(Permissions.CanReadComment);

            var result = await _commentRepository.GetById(request.CommentId, cancellationToken: cancellationToken);
            return result.ToCommentDto();
        }
    }
}