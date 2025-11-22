using Common;
using MediatR;
using UserService.Application.Models;
using UserService.Domain.Repos;

namespace UserService.Application.Queries;

public sealed record GetUsersByIdsQuery(List<Guid> UserIds) : IRequest<Result<List<UserModel>>>;

public sealed class GetUsersByIdsQueryHandler(IUserRepository repo)
    : IRequestHandler<GetUsersByIdsQuery, Result<List<UserModel>>>
{
    public async Task<Result<List<UserModel>>> Handle(GetUsersByIdsQuery request, CancellationToken ct)
    {
        var users = await repo.GetUsersByIdsReadonlyAsync(request.UserIds, ct);
        
        var userModels = users.Select(u => u.ToAppModel()).ToList();
        
        return userModels;
    }
}