using Common;
using MediatR;
using UserService.Domain.Entities;
using UserService.Domain.Repos;

namespace UserService.Application.Queries;

public sealed record GetAllUsersQuery(
    Guid CurrentUserId,
    bool? IsCurrentUserIncluded = false) : IRequest<Result<List<User>>>;

public sealed class GetAllUsersQueryHandler(IUserRepository userRepository) : IRequestHandler<GetAllUsersQuery, Result<List<User>>>
{
    public async Task<Result<List<User>>> Handle(GetAllUsersQuery request, CancellationToken ct)
    {
        var currentUser = await userRepository.GetUserReadonlyByIdAsync(request.CurrentUserId, ct);
        if (currentUser == null)
            return Result<List<User>>.Failure(Error.NotFound($"Пользователь с id {request.CurrentUserId} не найден."));

        var userTeamName = currentUser.TeamName;
        var users = await userRepository.GetAllUsersReadonlyAsync(
            request.CurrentUserId,
            userTeamName,
            request.IsCurrentUserIncluded ?? false,
            ct);

        return Result<List<User>>.Success(users);
    }
}
