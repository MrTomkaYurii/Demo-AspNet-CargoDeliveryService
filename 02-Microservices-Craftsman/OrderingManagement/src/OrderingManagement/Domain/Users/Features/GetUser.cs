namespace OrderingManagement.Domain.Users.Features;

using OrderingManagement.Domain.Users.Dtos;
using OrderingManagement.Domain.Users.Services;
using OrderingManagement.Exceptions;
using OrderingManagement.Domain;
using HeimGuard;
using Mappings;
using MediatR;

public static class GetUser
{
    public sealed record Query(Guid UserId) : IRequest<UserDto>;

    public sealed class Handler : IRequestHandler<Query, UserDto>
    {
        private readonly IUserRepository _userRepository;
        private readonly IHeimGuardClient _heimGuard;

        public Handler(IUserRepository userRepository, IHeimGuardClient heimGuard)
        {
            _userRepository = userRepository;
            _heimGuard = heimGuard;
        }

        public async Task<UserDto> Handle(Query request, CancellationToken cancellationToken)
        {
            // [AUTH-DISABLED] await _heimGuard.MustHavePermission<ForbiddenAccessException>(Permissions.CanReadUsers);

            var result = await _userRepository.GetById(request.UserId, cancellationToken: cancellationToken);
            return result.ToUserDto();
        }
    }
}