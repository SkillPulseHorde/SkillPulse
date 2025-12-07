using Common;
using MediatR;
using UserService.Domain.Repos;

namespace UserService.Application.Queries;

public sealed record GetUserIdByEmailQuery(string Email) : IRequest<Result<Guid>>;

public sealed class GetUserIdByEmailQueryHandler : IRequestHandler<GetUserIdByEmailQuery, Result<Guid>>
{
    private readonly IUserRepository _repo;

    public GetUserIdByEmailQueryHandler(IUserRepository repo)
    {
        _repo = repo;
    }

    public async Task<Result<Guid>> Handle(GetUserIdByEmailQuery request, CancellationToken ct)
    {
        var userId = await _repo.GetUserIdByEmailAsync(request.Email, ct);

        return userId is null
            ? Result<Guid>.Failure(Error.NotFound($"Пользователь с email {request.Email} не найден."))
            : Result<Guid>.Success(userId.Value);
    }
}
