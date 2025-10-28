using Common;
using MediatR;
using UserService.Application.Models;
using UserService.Domain.Entities;
using UserService.Domain.Repos;

namespace UserService.Application.Queries;

public sealed record GetAllUsersQuery(Guid CurrentUserId) : IRequest<Result<List<User>>>;

public sealed class GetAllUsersQueryHandler : IRequestHandler<GetAllUsersQuery, Result<List<User>>>
{
    private readonly IUserRepository _repo;

    public GetAllUsersQueryHandler(IUserRepository repo)
    {
        _repo = repo;
    }

    public async Task<Result<List<User>>> Handle(GetAllUsersQuery request, CancellationToken ct)
    {
        var currentUser = await _repo.GetUserReadonlyByIdAsync(request.CurrentUserId, ct);
        if (currentUser == null)
            return Result<List<User>>.Failure(Error.NotFound($"Пользователь с id {request.CurrentUserId} не найден."));
        
        var userTeamName = currentUser.TeamName;
        var users = await _repo.GetAllUsersReadonlyAsync(request.CurrentUserId, userTeamName, ct);

        return Result<List<User>>.Success(users);
    }
}
