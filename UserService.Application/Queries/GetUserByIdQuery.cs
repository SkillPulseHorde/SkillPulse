using Common;
using MediatR;
using UserService.Application.Models;
using UserService.Domain.Repos;

namespace UserService.Application.Queries;

public sealed record GetUserByIdQuery(Guid Id) : IRequest<Result<UserModel>>;

public sealed class GetUserByIdQueryHandler : IRequestHandler<GetUserByIdQuery, Result<UserModel>>
{
    private readonly IUserRepository _repo;

    public GetUserByIdQueryHandler(IUserRepository repo)
    {
        _repo = repo;
    }

    public async Task<Result<UserModel>> Handle(GetUserByIdQuery request, CancellationToken ct)
    {
        var user = await _repo.GetUserReadonlyByIdAsync(request.Id, ct);

        return user is null 
            ? Result<UserModel>.Failure(Error.NotFound($"Пользователь с id {request.Id} не найден.")) 
            : Result<UserModel>.Success(user.ToAppModel());
    }
}
