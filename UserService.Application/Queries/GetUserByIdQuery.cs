using Common;
using MediatR;
using UserService.Application.Dto;
using UserService.Domain.Repos;

namespace UserService.Application.Queries;

public sealed record GetUserByIdQuery(Guid Id) : IRequest<Result<UserDto>>;

public sealed class GetUserByIdQueryHandler : IRequestHandler<GetUserByIdQuery, Result<UserDto>>
{
    private readonly IUserRepository _repo;

    public GetUserByIdQueryHandler(IUserRepository repo)
    {
        _repo = repo;
    }

    public async Task<Result<UserDto>> Handle(GetUserByIdQuery request, CancellationToken ct)
    {
        var user = await _repo.GetUserByIdAsync(request.Id, ct);

        return user is null 
            ? Result<UserDto>.Failure(Error.NotFound($"User with id {request.Id} not found.")) 
            : Result<UserDto>.Success(user.ToDto());
    }
}
